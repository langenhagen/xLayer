Texture2D <float4> tex : register(t0);
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
    float4 ret = tex.Sample(TextureSampler, input.coords);


    return ret;
}
