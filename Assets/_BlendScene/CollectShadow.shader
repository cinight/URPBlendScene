Shader "CollectShadow"
{
   SubShader
   {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            //Basically a copy of CopyDepth shader but modified to blit with _BlitTexture instead of _CameraDepthAttachment
            Name "CollectRT_Shadow"
            ZTest Always ZWrite On ColorMask R
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_Edited
            #pragma multi_compile _ _DEPTH_MSAA_2 _DEPTH_MSAA_4 _DEPTH_MSAA_8
            #pragma multi_compile _ _OUTPUT_DEPTH

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/CopyDepthPass.hlsl"

            #if MSAA_SAMPLES == 1
                SAMPLER(sampler_BlitTexture);
            #else
                DEPTH_TEXTURE_MS(_BlitTexture, MSAA_SAMPLES);
                float4 _BlitTexture_TexelSize;
            #endif
            
            float SampleDepthEdited(float2 uv, Texture2D tex, SamplerState sp, float4 texelSize)
            {
            #if MSAA_SAMPLES == 1
                return SAMPLE_DEPTH_TEXTURE(tex, sp, uv);
            #else
                int2 coord = int2(uv * texelSize.zw);
                float outDepth = DEPTH_DEFAULT_VALUE;

                UNITY_UNROLL
                for (int i = 0; i < MSAA_SAMPLES; ++i)
                    outDepth = DEPTH_OP(OAD_TEXTURE2D_MSAA(tex, coord, i), outDepth);
                return outDepth;
            #endif
            }

            #if defined(_OUTPUT_DEPTH)
            float frag_Edited(Varyings input) : SV_Depth
            #else
            float frag_Edited(Varyings input) : SV_Target
            #endif
            {
                float2 uv = input.texcoord.xy;
                float4 src_ts = 0;

                #if MSAA_SAMPLES == 1
                #else
                    src_ts = _BlitTexture_TexelSize;
                #endif

                float col = SampleDepthEdited(uv, _BlitTexture, sampler_BlitTexture, src_ts);

                return col;
            }

            ENDHLSL
        }
    }
}