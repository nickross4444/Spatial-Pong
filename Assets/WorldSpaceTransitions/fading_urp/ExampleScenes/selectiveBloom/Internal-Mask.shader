Shader "Hidden/Internal-Mask"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
		LOD 100
		
		Pass
		{
			Name "Mask"
			ZWrite On
			HLSLPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			inline float DecodeFloatRG(float2 enc)
			{
				float2 kDecodeDot = float2(1.0, 1 / 255.0);
				return dot(enc, kDecodeDot);
			}

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float2 uv           : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				half4 positionCS    : SV_POSITION;
				half2 uv            : TEXCOORD0;
				float linearDepth : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 positionWS : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vertex(Attributes input)
			{
				Varyings output = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = input.uv;
				output.screenPos = ComputeScreenPos(output.positionCS);
				float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
				output.linearDepth = -(TransformWorldToView(worldPos).z * _ProjectionParams.w);
				return output;
			}

			TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);

			half4 Fragment(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				//half4 col = SAMPLE_TEXTURE2D(_BackfaceMaskTexture, sampler_BackfaceMaskTexture, input.uv);
				half4 col = half4(1, 1, 1, 1);
				// decode depth texture info
				float2 uv = input.screenPos.xy / input.screenPos.w; // normalized screen-space pos
				float camDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
				//float camDepth = DecodeFloatRG(enc.zw);
				camDepth = Linear01Depth(camDepth, _ZBufferParams);
				float diff = saturate(input.linearDepth - camDepth);
				if (diff < 0.00001)
				{
					col = half4(0, 0, 0, 1);
				}
				return col;

			}
		ENDHLSL
		}
	}
		Fallback Off
}
