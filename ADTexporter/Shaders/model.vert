#version 330
in vec3 vPosition;
in vec2 vTexCoord;

out vec2 vTexCoordOut;

uniform mat4 projection;
uniform mat4 modelview;
uniform mat4 rotation;
uniform mat4 translation;
uniform mat4 worldRotation;

void main()
{
    gl_Position = projection * modelview * translation * rotation * vec4(vPosition, 1.0);
    vTexCoordOut = vTexCoord;
}