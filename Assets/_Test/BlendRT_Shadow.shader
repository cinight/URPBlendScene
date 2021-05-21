Shader "Custom/BlendRT_Shadow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Blend ("Blend", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Name "CopyDepth"
            ZTest Always ZWrite On ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert_Test
            #pragma fragment frag_Test

            #pragma multi_compile _ _DEPTH_MSAA_2 _DEPTH_MSAA_4 _DEPTH_MSAA_8
            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/CopyDepthPass.hlsl"
            #include "Blending.hlsl"

            #if MSAA_SAMPLES == 1
                DEPTH_TEXTURE(Cam1_ShadowTexture);
                SAMPLER(sampler_Cam1_ShadowTexture);
                DEPTH_TEXTURE(Cam2_ShadowTexture);
                SAMPLER(sampler_Cam2_ShadowTexture);
            #else
                DEPTH_TEXTURE_MS(Cam1_ShadowTexture, MSAA_SAMPLES);
                float4 Cam1_ShadowTexture_TexelSize;
                DEPTH_TEXTURE_MS(Cam2_ShadowTexture, MSAA_SAMPLES);
                float4 Cam2_ShadowTexture_TexelSize;
            #endif

            float _Blend;

            Varyings vert_Test(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionHCS.xyz);//float4(input.positionHCS.xyz, 1.0);
                output.uv = input.uv;

                return output;
            }

            float frag_Test(Varyings input) : SV_Depth
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float col1 = SAMPLE_DEPTH_TEXTURE(Cam1_ShadowTexture, sampler_Cam1_ShadowTexture, input.uv);
                float col2 = SAMPLE_DEPTH_TEXTURE(Cam2_ShadowTexture, sampler_Cam2_ShadowTexture, input.uv);

                return Blending(col1,col2,_Blend).r;
            }

            ENDHLSL
        }
    }
}
