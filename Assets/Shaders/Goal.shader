Shader "Custom/Goal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Edge Color", Color) = (1,1,1,1)
        _GradientPower("Gradient Power", Range(0.1, 5)) = 1
        _ScrollDirection ("Scroll Direction", float) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
            "Queue"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Back
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO UNITY_STEREO_INSTANCING_ENABLED UNITY_STEREO_MULTIVIEW_ENABLED

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _GradientPower;
            half2 _ScrollDirection;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv += _ScrollDirection * _Time.y;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                // Calculate distance from center (0.5, 0.5)
                float2 centeredUV = abs(i.uv - 0.5) * 2;  // This makes UV go from -1 to 1 centered at 0
                float distFromCenter = max(centeredUV.x, centeredUV.y);  // Use max to create a square gradient
                
                // Calculate edge factor (1 at edges, 0 at center)
                float edge = pow(distFromCenter, _GradientPower);
                
                // Sample texture and apply color
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Final color with alpha
                fixed4 final = texColor * _Color;
                final.a *= edge;  // Make it transparent in center
                
                return final;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
