Shader "Custom/BlendRT_Depth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct fout
            {
                half4 color : SV_Target;
                float depth : DEPTH;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float _Blend;

            sampler2D _MainTex;
            sampler2D Cam1_CameraDepthTexture;
            sampler2D Cam2_CameraDepthTexture;

            fout frag (v2f i)
            {
                float4 col1 = tex2D(Cam1_CameraDepthTexture, i.uv);
                float4 col2 = tex2D(Cam2_CameraDepthTexture, i.uv);

                fout o;
                o.color = lerp(col1,col2,_Blend);
                o.depth = o.color.r;

                return o;
            }
            ENDCG
        }
    }
}
