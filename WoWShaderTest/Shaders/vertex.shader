#version 150

uniform mat4 modelview_matrix;
uniform mat4 projection_matrix;

in vec3 position;
in vec2 texCoord;

out vec2 TexCoord;

void main()
{
	gl_Position = projection_matrix * modelview_matrix * vec4(position, 1);
	TexCoord = texCoord;
}