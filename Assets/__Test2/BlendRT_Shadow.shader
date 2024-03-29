Shader "Custom/BlendRT_Shadow"
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

            DEPTH_TEXTURE(Cam1_ShadowTexture);
            SAMPLER(sampler_Cam1_ShadowTexture);
            DEPTH_TEXTURE(Cam2_ShadowTexture);
            SAMPLER(sampler_Cam2_ShadowTexture);

            float _Blend;

            Varyings vert_Test(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                //ref to Blit.hlsl
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);

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

                float col1 = SAMPLE_DEPTH_TEXTURE(Cam1_ShadowTexture, sampler_Cam1_ShadowTexture, input.texcoord);
                float col2 = SAMPLE_DEPTH_TEXTURE(Cam2_ShadowTexture, sampler_Cam2_ShadowTexture, input.texcoord);

                float col = BlendingShadow(col1,col2,_Blend,input.texcoord).r;

                fout o;
                o.color = col;
                o.depth = col;
                return o;
            }

            ENDHLSL
        }
    }
}
