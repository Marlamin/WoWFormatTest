#version 330

in vec2 TexCoord;
in vec4 VColor;
out vec4 outColor;

uniform sampler2D layer0;
uniform sampler2D layer1;
uniform sampler2D layer2;
uniform sampler2D layer3;

uniform float layer0scale;
uniform float layer1scale;
uniform float layer2scale;
uniform float layer3scale;

uniform sampler2D alphaLayer1;
uniform sampler2D alphaLayer2;
uniform sampler2D alphaLayer3;

void main()
{
	vec3 texture0 = texture(layer0, TexCoord * (8.0 / layer0scale)).rgb;
	vec3 texture1 = texture(layer1, TexCoord * (8.0 / layer1scale)).rgb;
	vec3 texture2 = texture(layer2, TexCoord * (8.0 / layer2scale)).rgb;
	vec3 texture3 = texture(layer3, TexCoord * (8.0 / layer3scale)).rgb;

	vec3 alphaTexture1 = texture(alphaLayer1, TexCoord).rgb;
	vec3 alphaTexture2 = texture(alphaLayer2, TexCoord).rgb;
	vec3 alphaTexture3 = texture(alphaLayer3, TexCoord).rgb;

	outColor = VColor * vec4(texture0 * (1.0 - (alphaTexture1 + alphaTexture2 + alphaTexture3)) + texture1 * alphaTexture1 + texture2 * alphaTexture2 + texture3 * alphaTexture3, 1.0);
}