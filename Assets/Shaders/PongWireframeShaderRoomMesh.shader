Shader "Unlit/PongWireframeShaderRoomMesh" {
  Properties {
    _WireframeColor("WireframeColor", Color) = (1, 0, 0, 1)
    _Color("Color", Color) = (1, 1, 1, 1)
    _MainTex ("Grid Texture", 2D) = "white" {}
    _TileSize("Tile Size", Float) = 1.0  // Controls how many times the texture repeats
  }

  SubShader {
    Pass {
      CGPROGRAM
      #include "UnityCG.cginc"
      #pragma vertex vert
      #pragma fragment frag

      half4 _WireframeColor, _Color;
      sampler2D _MainTex;
      float4 _MainTex_ST;
      float _TileSize;

      struct appdata
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      v2f vert(appdata v)
      {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_OUTPUT(v2f, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.vertex = UnityObjectToClipPos(v.vertex);
        
        // Convert world position and normal
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        float3 worldNormal = UnityObjectToWorldNormal(v.normal);
        
        // Get the absolute values of the normal
        float3 absNormal = abs(worldNormal);
        
        // Choose UV coordinates based on dominant axis
        float2 uv;
        if(absNormal.y > absNormal.x && absNormal.y > absNormal.z) {
            // Floor/ceiling
            uv = worldPos.xz;
        }
        else if(absNormal.x > absNormal.z) {
            // Walls aligned with X axis
            uv = worldPos.zy;
        }
        else {
            // Walls aligned with Z axis
            uv = worldPos.xy;
        }
        
        o.uv = uv * _TileSize;
        return o;
      }

      fixed4 frag(v2f i) : SV_Target
      {
        // Sample the texture
        fixed4 gridPattern = tex2D(_MainTex, i.uv);
        
        // Blend between base color and wireframe color based on the texture
        return lerp(_Color, _WireframeColor, gridPattern.r);
      }
      ENDCG
    }
  }
}
