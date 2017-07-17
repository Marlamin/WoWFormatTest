#version 330

in vec2 TexCoord;
out vec4 outColor;

uniform sampler2D colorTexture;

void main()
{
	// nd_winterorc_tower.wmo is a good candidate for alpha testing textures
	vec4 colTexture = texture(colorTexture, TexCoord);
	outColor = colTexture;
}