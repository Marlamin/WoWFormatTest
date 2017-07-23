#version 330

in vec2 TexCoord;
out vec4 outColor;


uniform sampler2D layer0;
uniform sampler2D layer1;
uniform sampler2D layer2;
uniform sampler2D layer3;
uniform sampler2D alphaLayer1;
uniform sampler2D alphaLayer2;
uniform sampler2D alphaLayer3;

void main()
{
	vec4 texture0 = texture(layer0, TexCoord);
	vec4 texture1 = texture(layer1, TexCoord);
	vec4 texture2 = texture(layer2, TexCoord);
	vec4 texture3 = texture(layer3, TexCoord);

	vec4 alphaTexture1 = texture(alphaLayer1, TexCoord);
	vec4 alphaTexture2 = texture(alphaLayer2, TexCoord);
	vec4 alphaTexture3 = texture(alphaLayer3, TexCoord);

	outColor = texture0 * (1.0 - (alphaTexture1 + alphaTexture2 + alphaTexture3)) + texture1 * alphaTexture1 + texture2 * alphaTexture2 + texture3 * alphaTexture3;
}