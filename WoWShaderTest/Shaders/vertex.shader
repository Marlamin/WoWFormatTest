#version 150

in vec2 position;
in vec2 texCoord;

out vec2 TexCoord;

void main()
{
	gl_Position = vec4(position, 0.0, 1.0);
	TexCoord = texCoord;
}