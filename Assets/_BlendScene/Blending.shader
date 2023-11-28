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
   }
}