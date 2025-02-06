// Copies pixels above a brightness threshold, and within a mask
// Used for per-object bloom

Shader "Hidden/MaskedBrightnessBlit"
{
	Properties
	{
		//_MainTex("Texture", 2D) = "white"
		_BloomThreshold("Bloom Threshold", Float) = 0.5
	}

	SubShader
	{
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

		Pass
		{
            Name "MaskedBrightnessPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Fullscreen.hlsl file provides the vertex shader (FullscreenVert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);
			//float4 _MainTex_ST;
			TEXTURE2D(_PerObjectBloomMask);
			half _BloomThreshold;

			half4 frag(Varyings input) : SV_Target
			{
				half4 col = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, input.texcoord);
				half4 mask = SAMPLE_TEXTURE2D(_PerObjectBloomMask, sampler_CameraColorTexture, input.texcoord);
				float brightness = dot(col.rgb, half3(0.2126, 0.7152, 0.0722));
				col.rgb = lerp(col.rgb, half3(0,0,0), mask.r); // Apply the mask
				col.rgb = step(_BloomThreshold, brightness) * col.rgb; // Cut off below the threshold
				return col;
			}
			ENDHLSL
		}
	}
}
