#version 330

in vec2 vTexCoordOut;
in vec2 vTexCoordAlphaOut;
in vec3 vColorOut;

out vec4 outColor;

uniform sampler2D layer0;
uniform sampler2D layer1;
uniform sampler2D layer2;
uniform sampler2D layer3;
uniform sampler2D blendLayer1;
uniform sampler2D blendLayer2;
uniform sampler2D blendLayer3;

void main()
{
    outColor = texture2D(layer0, vTexCoordOut) * (1.0 - (texture2D(blendLayer1, vTexCoordAlphaOut) + texture2D(blendLayer2, vTexCoordAlphaOut) + texture2D(blendLayer3, vTexCoordAlphaOut))) + texture2D(layer1, vTexCoordOut) * texture2D(blendLayer1, vTexCoordAlphaOut) + texture2D(layer2, vTexCoordOut) * texture2D(blendLayer2, vTexCoordAlphaOut) + texture2D(layer3, vTexCoordOut) * texture2D(blendLayer3, vTexCoordAlphaOut);
}