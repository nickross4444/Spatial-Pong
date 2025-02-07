Shader "Custom/SceneMeshGridTransition"
{
    Properties
    {
        _Height ("Transition Height", Float) = 0
        _FadeSmoothness ("Fade Smoothness", Float) = 0.1
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _WireframeColor("Wireframe Color", Color) = (1, 0, 0, 1)
        _MainTex ("Grid Texture", 2D) = "white" {}
        _TileSize("Tile Size", Float) = 1.0
        _BlendSharpness("Blend Sharpness", Float) = 5.0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Back  // Hide backfaces (prevents backside rendering)
            ZWrite On  // Ensures correct depth rendering
            ZTest LEqual // Proper depth testing
            Blend One Zero // Ensures full opacity

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 objectPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _Height;
            float _FadeSmoothness;
            float4 _BaseColor;
            float4 _WireframeColor;
            sampler2D _MainTex;
            float _TileSize;
            float _BlendSharpness;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.position = UnityObjectToClipPos(v.vertex);
                o.objectPos = v.vertex.xyz; // Use Object Space Position
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float triplanarTexture(float3 objectPos, float3 worldNormal, float tileSize)
            {
                float3 alignedPos = objectPos * tileSize;
                float3 absNormal = abs(worldNormal);
                float3 blendWeights = pow(absNormal, _BlendSharpness);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z + 0.0001);

                float2 uvX = alignedPos.zy;
                float2 uvY = alignedPos.xz;
                float2 uvZ = alignedPos.xy;

                float sampled =
                    tex2D(_MainTex, uvX).r * blendWeights.x +
                    tex2D(_MainTex, uvY).r * blendWeights.y +
                    tex2D(_MainTex, uvZ).r * blendWeights.z;

                return sampled;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Use Object Space Y for transition (0 = bottom, 1 = top)
                float fade = smoothstep(-_Height - _FadeSmoothness, -_Height + _FadeSmoothness, -i.objectPos.y);

                // Clip pixels that are below the transition (prevents rendering behind)
                clip(fade - 0.001);

                // Get triplanar wireframe grid pattern
                float gridPattern = triplanarTexture(i.objectPos, i.worldNormal, _TileSize);
                
                // Blend between base color and wireframe color
                float3 blendedColor = lerp(_BaseColor.rgb, _WireframeColor.rgb, gridPattern);

                return float4(blendedColor, 1.0); // Fully opaque
            }
            ENDCG
        }
    }
}
