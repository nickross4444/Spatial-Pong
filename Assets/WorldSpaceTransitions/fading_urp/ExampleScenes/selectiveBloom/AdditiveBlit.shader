Shader "Hidden/AdditiveBlit"
{
	Properties
	{
		//_MainTex("Texture", 2D) = "white" {}
		_AdditiveAmount("Additive Amount", Range(0, 5)) = 1.0
	}

	SubShader
	{
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

		// Additive blend
		Blend One One

		Pass
		{
            Name "AdditiveBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            //TEXTURE2D(_CameraColorTexture);
            //SAMPLER(sampler_CameraColorTexture);
			float _AdditiveAmount;

			half4 frag(Varyings input) : SV_Target
			{
				float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
				return color * _AdditiveAmount;
			}
			ENDHLSL
		}
	}
}
