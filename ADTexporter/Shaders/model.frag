#version 330

in vec2 vTexCoordOut;

out vec4 outColor;

uniform sampler2D layer0;

void main()
{
    outColor = texture2D(layer0, vTexCoordOut);
}