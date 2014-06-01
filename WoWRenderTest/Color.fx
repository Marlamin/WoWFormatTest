struct VS_IN
{
	float4 pos : POSITION;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
};

float4x4 worldViewProj;
float4 color;

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(input.pos, worldViewProj);
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return color;
}