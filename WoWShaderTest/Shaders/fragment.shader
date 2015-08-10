#version 150

in vec2 TexCoord;

out vec4 outColor;

uniform sampler2D shaneCube;
uniform sampler2D shaneCubeNew;

void main()
{
	vec4 colKitten = texture(shaneCube, TexCoord);
	vec4 colPuppy = texture(shaneCubeNew, TexCoord);
	outColor = mix(colKitten, colPuppy, 0.5);
}