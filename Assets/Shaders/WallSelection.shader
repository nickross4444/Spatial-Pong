Shader "MixedReality/WallSelection" {
    Properties {
        _Color ("Tint", Color) = (1,1,1,1)
        _Angle ("Effect Angle", Range (0, 90)) = 60
        _TextTex("Text Texture", 2D) = "white" {}
        _TextOpacity("Text Opacity", Range(0,1)) = 0.5
    }
    SubShader {
        PackageRequirements {"com.unity.render-pipelines.universal"}
        Pass {
            Tags { "Queue"="Transparent" "RenderQueue"="3001" "LightMode" = "UniversalForward"}
            LOD 100
            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : NORMAL;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct Varyings {
                float2 uv : TEXCOORD0;
                float4 color : TEXCOORD1;
                half3 normal : NORMAL;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;
            float _Angle;
            sampler2D _TextTex;
            float _TextOpacity;

            Varyings vert (Attributes input) {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = TransformObjectToHClip(input.vertex.xyz);
                o.worldPos = mul(unity_ObjectToWorld, input.vertex);
                o.normal = TransformObjectToWorldNormal(input.normal.xyz);
                
                // Use the plane's local UVs to keep text fixed
                o.uv = input.uv;

                o.color = input.color;
                return o;
            }

            half4 frag (Varyings i, float facing : VFACE) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float backFace = facing > 0 ? 1 : 0.1;

                // Sample text using the plane’s UV coordinates
                float4 textSample = tex2D(_TextTex, i.uv);
                textSample.a *= _TextOpacity;  // Control text transparency

                // Blend text with the rest of the effect
                half4 finalEffect = _Color * backFace;
                finalEffect = lerp(finalEffect, textSample, textSample.a); // Blend text

                return finalEffect;
            }
            ENDHLSL
        }
    }
}





