Shader "Custom/BlendRT_Depth"
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

            #if MSAA_SAMPLES == 1
                DEPTH_TEXTURE(Cam1_CameraDepthTexture);
                SAMPLER(sampler_Cam1_CameraDepthTexture);
                DEPTH_TEXTURE(Cam2_CameraDepthTexture);
                SAMPLER(sampler_Cam2_CameraDepthTexture);
            #else
                DEPTH_TEXTURE_MS(Cam1_CameraDepthTexture, MSAA_SAMPLES);
                float4 Cam1_CameraDepthTexture_TexelSize;
                DEPTH_TEXTURE_MS(Cam2_CameraDepthTexture, MSAA_SAMPLES);
                float4 Cam2_CameraDepthTexture_TexelSize;
            #endif

            float _Blend;

            Varyings vert_Test(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Note: CopyDepth pass is setup with a mesh already in CS
                // Therefore, we can just output vertex position

                // We need to handle y-flip in a way that all existing shaders using _ProjectionParams.x work.
                // Otherwise we get flipping issues like this one (case https://issuetracker.unity3d.com/issues/lwrp-depth-texture-flipy)

                // Unity flips projection matrix in non-OpenGL platforms and when rendering to a render texture.
                // If URP is rendering to RT:
                //  - Source Depth is upside down. We need to copy depth by using a shader that has flipped matrix as well so we have same orientaiton for source and copy depth.
                //  - This also guarantess to be standard across if we are using a depth prepass.
                //  - When shaders (including shader graph) render objects that sample depth they adjust uv sign with  _ProjectionParams.x. (https://docs.unity3d.com/Manual/SL-PlatformDifferences.html)
                //  - All good.
                // If URP is NOT rendering to RT neither rendering with OpenGL:
                //  - Source Depth is NOT fliped. We CANNOT flip when copying depth and don't flip when sampling. (ProjectionParams.x == 1)
           // #if _USE_DRAW_PROCEDURAL
               // output.positionCS = GetQuadVertexPosition(input.vertexID);
               // output.positionCS.xy = output.positionCS.xy * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f); //convert to -1..1
               // output.uv = GetQuadTexCoord(input.vertexID);
           // #else
                output.positionCS = TransformObjectToHClip(input.positionHCS.xyz);//float4(input.positionHCS.xyz, 1.0);
                output.uv = input.uv;
            //#endif
               // output.positionCS.y *= _ScaleBiasRt.x;
                return output;
            }

            float SampleDepth_Test(float2 uv)
            {
                float col1 = SAMPLE_DEPTH_TEXTURE(Cam1_CameraDepthTexture, sampler_Cam1_CameraDepthTexture, uv);
                float col2 = SAMPLE_DEPTH_TEXTURE(Cam2_CameraDepthTexture, sampler_Cam2_CameraDepthTexture, uv);

                return lerp(col1,col2,_Blend);
            // #if MSAA_SAMPLES == 1
            //     return SAMPLE_DEPTH_TEXTURE(_BlendDepthTexture, sampler_BlendDepthTexture, uv);
            // #else
            //     int2 coord = int2(uv * _BlendDepthTexture_TexelSize.zw);
            //     float outDepth = DEPTH_DEFAULT_VALUE;

            //     UNITY_UNROLL
            //     for (int i = 0; i < MSAA_SAMPLES; ++i)
            //         outDepth = DEPTH_OP(LOAD(coord, i), outDepth);
            //     return outDepth;
            // #endif
            }

            float frag_Test(Varyings input) : SV_Depth
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return SampleDepth_Test(input.uv);
            }

            ENDHLSL
        }
    }
}
