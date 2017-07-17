#version 330

in vec2 TexCoord;
out vec4 outColor;

uniform float alphaRef;
uniform sampler2D colorTexture;

void main()
{
	vec4 colTexture = texture(colorTexture, TexCoord);

	if (colTexture.a < alphaRef) { discard; }

	outColor = colTexture;
}