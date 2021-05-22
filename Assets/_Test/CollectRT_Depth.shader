Shader "Custom/CollectRT_Depth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                DEPTH_TEXTURE(_CameraDepthTexture);
                SAMPLER(sampler_CameraDepthTexture);
            #else
                DEPTH_TEXTURE_MS(_CameraDepthTexture, MSAA_SAMPLES);
                float4 _CameraDepthTexture_TexelSize;
            #endif

            Varyings vert_Test(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionHCS.xyz);//float4(input.positionHCS.xyz, 1.0);
                output.uv = input.uv;

                return output;
            }

            struct fout
            {
                float4 color : SV_Target;
                float depth : SV_Depth;
            };

            fout frag_Test(Varyings input)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float col = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv);

                fout o;
                o.color = col;
                o.depth = col;
                return o;
            }

            ENDHLSL
        }
    }
}
