#version 330

in vec2 TexCoord;
out vec4 outColor;

uniform sampler2D colorTexture;

void main()
{
	vec4 colTexture = texture(colorTexture, TexCoord);
	outColor = colTexture;
}