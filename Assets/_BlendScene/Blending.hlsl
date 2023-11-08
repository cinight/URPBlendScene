#ifndef BLENDING_INCLUDED
#define BLENDING_INCLUDED

#define pi 3.14159265358979

sampler2D Blended_CameraDepthTexture;
float SampleBlendedDepth01(float2 uv)
{
    float depth = tex2D(Blended_CameraDepthTexture, uv).r;
    float depth01 = 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y); // Linear01Depth
    return depth01;
}

//returns a-mid-b, when k is 0-0.5-1
float4 MidLerp(float4 a, float4 b, float4 mid, float k)
{
    float m = sin( k * pi ); //when k is 0-0.5-1, returns 0-1-0
    float l = saturate( ( (1.0-k) * 2.0)-1.0 ); //when k is 0-0.5-1, returns 1-0-0
    float r = saturate( (k * 2.0)-1.0 ); //when k is 0-0.5-1, returns 0-0-1

    float4 L = lerp( 0 , a , l );
    float4 R = lerp( 0 , b , r );
    float4 M = lerp( 0 , mid , m );

    return L+R+M;
}

float3 Contrast(float3 col, float k) //k=0 to 2
{
return ((col - 0.5f) * max(k, 0)) + 0.5f;
}

float4 BlendingColor(float4 a, float4 b, float k, float2 uv)
{
    //Grey color
    float dist = distance(uv,0.5);
    dist = 1- dist;
    float4 mid = lerp( a , b , k );
    float bw = (mid.r + mid.g + mid.b) / (3.0*dist);
    mid.rgb = 1-bw;

    return MidLerp ( a , b , mid , k );
}

float4 BlendingGBuffer3(float4 a, float4 b, float k, float2 uv)
{
    //Edge
    float dt = 0.0005f;
    float depthC = SampleBlendedDepth01(float2(uv.x,uv.y));
    float depthL = SampleBlendedDepth01(float2(uv.x-dt,uv.y));
    float depthR = SampleBlendedDepth01(float2(uv.x+dt,uv.y));
    float depthT = SampleBlendedDepth01(float2(uv.x,uv.y+dt));
    float depthB = SampleBlendedDepth01(float2(uv.x,uv.y-dt));
    float edge = abs(depthL-depthC) + abs(depthR-depthC) + abs(depthT-depthC) + abs(depthB-depthC);
    edge = Contrast(edge*100,2.0).r;
    edge *= 0.1f;

    float4 mid = lerp(a,b,k);
    mid.rgb = lerp(0,float3(0.2588235,0.2392157,0.1921569),edge);

    return MidLerp ( a , b , mid , k );
}

float4 BlendingDepth(float4 a, float4 b, float k, float2 uv)
{
    float4 mid = max(a , b);
    return MidLerp ( a , b , mid , k );
}

float4 BlendingShadow(float4 a, float4 b, float k, float2 uv)
{
    return lerp ( a , b , k );
}

#endif
