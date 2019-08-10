Texture2D <float4> tex;
Texture2D <float4> alphamap; // u cld change me 2 a float for smllr imgs & faster speed
                             // (but... then also change the descriptor within the cpu prog!)
sampler TextureSampler;

struct VS_IN
{
    float4 pos    : POSITION;
    float2 coords : TEXTCOORD;
};

struct PS_IN
{
    float4 pos    : SV_POSITION;
    float2 coords : TEXTCOORD;
};

PS_IN vertex_shader(VS_IN input)
{
    PS_IN output;
    output.pos    = input.pos;
    output.coords = input.coords;

    return output;
}

float4 pixel_shader(PS_IN input) : SV_Target
{
    float4 ret   = tex.Sample(TextureSampler, input.coords);
    float4 alpha = alphamap.Sample(TextureSampler, input.coords);

    float grayscale = dot(alpha.rgb, float3(0.3, 0.59, 0.11));

    ret.a = grayscale;

    return ret;
}
