Shader "Unlit/PongWireframeShader_All_In_One" { 
    Properties {
      _WireframeColor("Wireframe Color", Color) = (1, 0, 0, 1)
      _Color("Base Color", Color) = (1, 1, 1, 1)
      _MainTex("Grid Texture", 2D) = "white" {}
      _TileSize("Tile Size", Float) = 1.0 // Properly handles tiling
      _BlendSharpness("Blend Sharpness", Float) = 5.0 // Controls triplanar blending
      _Height("Transition Height (0-1)", Range(0, 1)) = 0.0 // Controls transition percentage
      _FadeSmoothness("Fade Smoothness", Float) = 0.1 // Softens the transition
      _MinWorldY("Min World Y", Float) = 0.0 // Minimum world space Y coordinate
      _MaxWorldY("Max World Y", Float) = 4.0 // Maximum world space Y coordinate
    }
  
    SubShader {
      Pass {
        CGPROGRAM
        #include "UnityCG.cginc"
        #pragma vertex vert
        #pragma fragment frag
  
        half4 _WireframeColor, _Color;
        sampler2D _MainTex;
        float _TileSize;
        float _BlendSharpness;
        float _Height;
        float _FadeSmoothness;
        float _MinWorldY;
        float _MaxWorldY;
  
        struct appdata {
          float4 vertex : POSITION;
          float3 normal : NORMAL;
          UNITY_VERTEX_INPUT_INSTANCE_ID
        };
  
        struct v2f {
          float4 vertex : SV_POSITION;
          float3 worldPos : TEXCOORD0;
          float3 worldNormal : TEXCOORD1;
          UNITY_VERTEX_OUTPUT_STEREO
        };
  
        v2f vert(appdata v) {
          v2f o;
          UNITY_SETUP_INSTANCE_ID(v);
          UNITY_INITIALIZE_OUTPUT(v2f, o);
          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
          
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
          o.worldNormal = UnityObjectToWorldNormal(v.normal);
          return o;
        }
  
        float triplanarTexture(float3 worldPos, float3 worldNormal, float tileSize) {
          // Fix alignment by dividing instead of multiplying
          float3 alignedPos = worldPos / tileSize;
  
          // Calculate blending weights based on normal
          float3 absNormal = abs(worldNormal);
          float3 blendWeights = pow(absNormal, _BlendSharpness);
          blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z + 0.0001); // Normalize
  
          // Compute UVs for each projection plane
          float2 uvX = alignedPos.zy;
          float2 uvY = alignedPos.xz;
          float2 uvZ = alignedPos.xy;
  
          // Sample triplanar textures and blend them
          float sampled =
              tex2D(_MainTex, uvX).r * blendWeights.x +
              tex2D(_MainTex, uvY).r * blendWeights.y +
              tex2D(_MainTex, uvZ).r * blendWeights.z;
  
          return sampled;
        }
  
        fixed4 frag(v2f i) : SV_Target {
          // Calculate fade factor using world space height
          float normalizedHeight = (i.worldPos.y - _MinWorldY) / (_MaxWorldY - _MinWorldY);
          float fade = 1 - smoothstep(_Height - _FadeSmoothness, _Height + _FadeSmoothness, normalizedHeight);
  
          // Clip pixels above the transition height
          clip(fade - 0.001);
  
          // Apply triplanar-mapped texture
          float gridPattern = triplanarTexture(i.worldPos, i.worldNormal, _TileSize);
          
          // Blend between base color and wireframe color
          return lerp(_Color, _WireframeColor, gridPattern);
        }
        ENDCG
      }
    }
  }