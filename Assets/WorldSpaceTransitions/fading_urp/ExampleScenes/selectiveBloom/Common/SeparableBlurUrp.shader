Shader "Hidden/GlassBlur" {
	Properties
	{
		//_MainTex("Texture", 2D) = "white" {}
        _sigma ("sigma", Float) = 2.8
	}

	HLSLINCLUDE
	    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
        // The Fullscreen.hlsl file provides the vertex shader (FullscreenVert),
        // input structure (Attributes) and output strucutre (Varyings)
		#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

		float4 _BlitTexture_TexelSize;
        int _BlurStrength;
        float _sigma;

        #define PI 3.14159265

        float gauss(float x, float sigma)
        {
	        return  1.0f / (2.0f * PI * sigma * sigma) * exp(-(x * x) / (2.0f * sigma * sigma));
        }

		half4 FragHorizontal(Varyings input) : SV_TARGET
        {
            float2 res = _BlitTexture_TexelSize.xy;
            half4 sum = 0;
 
            int samples = 2 * _BlurStrength + 1;
            float weights = 0.0;
 
            for(float x = 0; x < samples; x++)
            {
                float2 offset = float2(x - _BlurStrength, 0);
                float weight = gauss(x - _BlurStrength, _sigma);// + gauss(x - _BlurStrength +1, _sigma);
                weights += weight;
                sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + offset * res)*weight;
            }
            //return sum / samples;
            return sum / weights;
        }
 
        half4 FragVertical(Varyings input) : SV_TARGET
        {
            float2 res = _BlitTexture_TexelSize.xy;
            half4 sum = 0;
 
            int samples = 2 * _BlurStrength + 1;
            float weights = 0.0;
 
            for(float y = 0; y < samples; y++)
            {
                float2 offset = float2(0, y - _BlurStrength);
                float weight = gauss(y - _BlurStrength, _sigma);// + gauss(y - _BlurStrength +1, _sigma);
                weights += weight;
                sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + offset * res)*weight;
            }
            //return sum / samples;
            return sum / weights;
        }



		float4 offsets;

		half4 frag(Varyings input) : SV_Target
		{
			half4 color = float4 (0,0,0,0);
			float2 Texel = _BlitTexture_TexelSize.xy;
			//Texel = float2(1,1);
			float2 uv11 = input.texcoord - offsets.xy*float2(Texel.x, Texel.y);
			float2 uv12 = input.texcoord + offsets.xy*float2(Texel.x, Texel.y);
			float2 uv21 = input.texcoord - offsets.xy*float2(Texel.x, Texel.y)*2;
			float2 uv22 = input.texcoord + offsets.xy*float2(Texel.x, Texel.y)*2;
			float2 uv31 = input.texcoord - offsets.xy*float2(Texel.x, Texel.y)*3;
			float2 uv32 = input.texcoord + offsets.xy*float2(Texel.x, Texel.y)*3;
					
			color += 0.40 * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
			color += 0.15 * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv11);
			color += 0.15 * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv12);
			color += 0.10 * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv21);
			color += 0.10 * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv22);
			color += 0.05 * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv31);
			color += 0.05 * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv32);

			return color;
		}
	ENDHLSL
	SubShader
	{
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
		Pass // 0
        {
            Name "Horizontal Box Blur"
 
            HLSLPROGRAM
 
            #pragma vertex Vert
            #pragma fragment FragHorizontal
 
            ENDHLSL
        }
   
        Pass // 1
        {
            Name "Vertical Box Blur"
 
            HLSLPROGRAM
 
            #pragma vertex Vert
            #pragma fragment FragVertical
 
            ENDHLSL
        }
		Pass // 2
		{
            Name "BlitPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

			ENDHLSL
		}
	}
} // shader
