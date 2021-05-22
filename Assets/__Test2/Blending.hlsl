#ifndef BLENDING_INCLUDED
#define BLENDING_INCLUDED

float4 Blending(float4 a, float4 b, float k)
{
    float4 col = lerp(a,b,k);
    return col;
}

#endif
