Shader "Custom/BlendRT_GBuffer3"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Blend ("Blend", Range(0,1)) = 0.5
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Blending.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float _Blend;

            //sampler2D _MainTex;
            sampler2D Cam1_GBuffer3;
            sampler2D Cam2_GBuffer3;

            float4 frag (v2f i) : SV_Target
            {
                float4 col1 = tex2D(Cam1_GBuffer3, i.uv);
                float4 col2 = tex2D(Cam2_GBuffer3, i.uv);

                float4 col = Blending(col1,col2,_Blend);
                return col;
            }
            ENDCG
        }
    }
}
