#version 150

in vec2 TexCoord;

out vec4 outColor;

uniform sampler2D shaneCube;

void main()
{
	vec4 colKitten = texture(shaneCube, TexCoord);
	outColor = colKitten;
}