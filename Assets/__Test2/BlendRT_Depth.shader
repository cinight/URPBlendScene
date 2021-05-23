Shader "Custom/BlendRT_Depth"
{
    Properties
    {
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

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/CopyDepthPass.hlsl"
            #include "Blending.hlsl"

            DEPTH_TEXTURE(Cam1_CameraDepthTexture);
            SAMPLER(sampler_Cam1_CameraDepthTexture);
            DEPTH_TEXTURE(Cam2_CameraDepthTexture);
            SAMPLER(sampler_Cam2_CameraDepthTexture);

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

            struct fout
            {
                float4 color : SV_Target;
                float depth : SV_Depth;
            };

            fout frag_Test(Varyings input)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float col1 = SAMPLE_DEPTH_TEXTURE(Cam1_CameraDepthTexture, sampler_Cam1_CameraDepthTexture, input.uv);
                float col2 = SAMPLE_DEPTH_TEXTURE(Cam2_CameraDepthTexture, sampler_Cam2_CameraDepthTexture, input.uv);

                float col = BlendingDepth(col1,col2,_Blend,input.uv).r;

                fout o;
                o.color = col;
                o.depth = col;
                return o;
            }

            ENDHLSL
        }
    }
}
