Shader "Blending"
{
    Properties
    {
        _Blend ("Blend", Range(0,1)) = 0.5
    }
   SubShader
   {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       
        Pass
        {
            Name "BlendRT"
            ZWrite Off Cull Off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Blending.hlsl"

            #pragma vertex Vert
            #pragma fragment frag
            
            TEXTURE2D_X(_Src1);
            SAMPLER(sampler_Src1);

            TEXTURE2D_X(_Src2);
            SAMPLER(sampler_Src2);

            float _Blend;

            float4 frag (Varyings input) : SV_Target0
            {
                float2 uv = input.texcoord.xy;

                float4 col1 = SAMPLE_TEXTURE2D_X(_Src1, sampler_Src1, uv);
                float4 col2 = SAMPLE_TEXTURE2D_X(_Src2, sampler_Src2, uv);
                
                float4 col = BlendingColor(col1, col2, _Blend, uv);

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "BlendRT_Depth"
            ZTest Always ZWrite On ColorMask R
            Cull Off
            
            //Only used for the depth texture
            //Observe values from Cam1 or Cam2 GBuffer pass stencil settings on FrameDebugger
            Stencil
            {
                Ref 32
                ReadMask 0
                WriteMask 96
                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_Edited
            #pragma multi_compile _ _DEPTH_MSAA_2 _DEPTH_MSAA_4 _DEPTH_MSAA_8
            #pragma multi_compile _ _OUTPUT_DEPTH

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/CopyDepthPass.hlsl"
            #include "Blending.hlsl"

            float _Blend;

            #if MSAA_SAMPLES == 1
                DEPTH_TEXTURE(_Src1);
                SAMPLER(sampler_Src1);
                DEPTH_TEXTURE(_Src2);
                SAMPLER(sampler_Src2);
            #else
                DEPTH_TEXTURE_MS(_Src1, MSAA_SAMPLES);
                float4 _Src1_TexelSize;
                DEPTH_TEXTURE_MS(_Src2, MSAA_SAMPLES);
                float4 _Src2_TexelSize;
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


                float4 src1_ts = 0;
                float4 src2_ts = 0;

                #if MSAA_SAMPLES == 1
                #else
                    src1_ts = _Src1_TexelSize;
                    src2_ts = _Src2_TexelSize;
                #endif
                
                float col1 = SampleDepthEdited(uv, _Src1, sampler_Src1, src1_ts);
                float col2 = SampleDepthEdited(uv, _Src2, sampler_Src2, src2_ts);
                
                float col = BlendingShadow(col1, col2, _Blend, uv).r;

                return col;
            }

            ENDHLSL
        }
   }
}