struct VS_IN
{
	float4 pos : POSITION;
	float2 tex : TEXCOORD0;
	float2 map : TEXCOORD1;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD0;
	float2 map : TEXCOORD1;
};

float4x4 worldViewProj;
SamplerState pictureSampler[2];

Texture2D layer[5];

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(input.pos, worldViewProj);
	output.tex = input.tex;
	output.map = input.map;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	float4 color[5];
	float4 ret;

	color[0] = layer[0].Sample(pictureSampler[1], input.tex);
	color[1] = layer[1].Sample(pictureSampler[1], input.tex);
	color[2] = layer[2].Sample(pictureSampler[1], input.tex);
	color[3] = layer[3].Sample(pictureSampler[1], input.tex);
	color[4] = layer[4].Sample(pictureSampler[0], input.map);

	//ret = lerp(color[0],color[3],color[4][0]);
	//ret = lerp(color[0],color[1],color[4][2]);
	//ret = lerp(color[0],color[2],color[4][1]);

	ret = lerp(color[0], color[3], color[4][0]);
	ret = lerp(ret, color[1], color[4][2]);
	ret = lerp(ret, color[2], color[4][1]);

	//return color[4];
	return ret;
}