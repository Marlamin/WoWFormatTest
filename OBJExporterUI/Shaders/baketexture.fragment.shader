#version 330

in vec2 TexCoord;
in vec4 VColor;
out vec4 out_col0;

uniform vec4 pc_heightScale;
uniform vec4 pc_heightOffset;

uniform float layer0scale;
uniform float layer1scale;
uniform float layer2scale;
uniform float layer3scale;

uniform sampler2D pt_layer0;
uniform sampler2D pt_layer1;
uniform sampler2D pt_layer2;
uniform sampler2D pt_layer3;

uniform sampler2D pt_blend1;
uniform sampler2D pt_blend2;
uniform sampler2D pt_blend3;

uniform sampler2D pt_height0;
uniform sampler2D pt_height1;
uniform sampler2D pt_height2;
uniform sampler2D pt_height3;

void main()
{

	vec2 tc0 = TexCoord * (8.0 / layer0scale);
	vec2 tc1 = TexCoord * (8.0 / layer1scale);
	vec2 tc2 = TexCoord * (8.0 / layer2scale);
	vec2 tc3 = TexCoord * (8.0 / layer3scale);
	
	vec4 in_vertexColor = VColor;

	float blendTex0 = texture(pt_blend1, TexCoord).r;
	float blendTex1 = texture(pt_blend2, TexCoord).r;
	float blendTex2 = texture(pt_blend3, TexCoord).r;
	vec3 blendTex = vec3(blendTex0, blendTex1, blendTex2);

	vec4 layer_weights = vec4(1.0 - clamp(dot(vec3(1.0), blendTex), 0, 1), blendTex);
	vec4 layer_pct = vec4(layer_weights.x * (texture(pt_height0, tc0).a * pc_heightScale[0] + pc_heightOffset[0])
		, layer_weights.y * (texture(pt_height1, tc1).a * pc_heightScale[1] + pc_heightOffset[1])
		, layer_weights.z * (texture(pt_height2, tc2).a * pc_heightScale[2] + pc_heightOffset[2])
		, layer_weights.w * (texture(pt_height3, tc3).a * pc_heightScale[3] + pc_heightOffset[3])
	);

	vec4 layer_pct_max = vec4(max(max(layer_pct.x, layer_pct.y), max(layer_pct.y, layer_pct.z)));                              
	layer_pct = layer_pct * (vec4(1.0) - clamp(layer_pct_max - layer_pct, 0, 1));
	layer_pct = layer_pct / vec4(dot(vec4(1.0), layer_pct));

	vec4 weightedLayer_0 = texture(pt_layer0, tc0) * layer_pct.x;
	vec4 weightedLayer_1 = texture(pt_layer1, tc1) * layer_pct.y;
	vec4 weightedLayer_2 = texture(pt_layer2, tc2) * layer_pct.z;
	vec4 weightedLayer_3 = texture(pt_layer3, tc3) * layer_pct.w;

	out_col0 = vec4((weightedLayer_0.xyz + weightedLayer_1.xyz + weightedLayer_2.xyz + weightedLayer_3.xyz) * in_vertexColor.rgb * 2.0, 1.0);
}