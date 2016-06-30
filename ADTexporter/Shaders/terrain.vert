#version 330
in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoord;
in vec2 vTexCoordAlpha;
in vec3 vColor;

out vec2 vTexCoordOut;
out vec2 vTexCoordAlphaOut;
out vec3 vColorOut;

uniform mat4 projection;
uniform mat4 modelview;

void main()
{
    gl_Position = projection * modelview * vec4(vPosition, 1.0);
    vColorOut = vColor;
    vTexCoordOut = vTexCoord;
    vTexCoordAlphaOut = vTexCoordAlpha;
}