Shader "Unlit/PongWireframeShaderRoomMeshTransition" {  
  Properties {
    _WireframeColor("Wireframe Color", Color) = (1, 0, 0, 1)
    _Color("Base Color", Color) = (1, 1, 1, 1)
    _MainTex("Grid Texture", 2D) = "white" {}
    _TileSize("Tile Size", Float) = 1.0 // Controls grid scale
    _BlendSharpness("Blend Sharpness", Float) = 5.0 // Triplanar blend sharpness
    _Height("Transition Height", Float) = 0.0 // Controls where transition occurs
    _FadeSmoothness("Fade Smoothness", Float) = 0.1 // Softens the transition
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

      struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        float3 objectPos : TEXCOORD0;  // Using object space for height transition
        float3 worldNormal : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      v2f vert(appdata v) {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_OUTPUT(v2f, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.objectPos = v.vertex.xyz; // Object space position for height transition
        o.worldNormal = UnityObjectToWorldNormal(v.normal);

        return o;
      }

      float triplanarTexture(float3 worldPos, float3 worldNormal, float tileSize) {
        float3 alignedPos = worldPos / tileSize;
        float3 absNormal = abs(worldNormal);
        float3 blendWeights = pow(absNormal, _BlendSharpness);
        blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z + 0.0001); // Normalize

        float2 uvX = alignedPos.zy;
        float2 uvY = alignedPos.xz;
        float2 uvZ = alignedPos.xy;

        float sampled =
            tex2D(_MainTex, uvX).r * blendWeights.x +
            tex2D(_MainTex, uvY).r * blendWeights.y +
            tex2D(_MainTex, uvZ).r * blendWeights.z;

        return sampled;
      }

      fixed4 frag(v2f i) : SV_Target {
        // Calculate fade factor using object space Y coordinate
        float fade = smoothstep(-_Height - _FadeSmoothness, -_Height + _FadeSmoothness, -i.objectPos.y);

        // Clip pixels below the transition height
        clip(fade - 0.001);

        // Apply triplanar-mapped grid pattern
        float gridPattern = triplanarTexture(i.objectPos, i.worldNormal, _TileSize);

        // Blend between base color and wireframe color
        float3 blendedColor = lerp(_Color.rgb, _WireframeColor.rgb, gridPattern);

        return float4(blendedColor, 1.0);
      }
      ENDCG
    }
  }
}
