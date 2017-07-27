using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.Structs.BLS
{
    public struct BLS
    {
        public uint version;
        public uint permutationCount;
        public uint nShaders;
        public uint ofsCompressedChunks;
        public uint nCompressedChunks;
        public uint ofsCompressedData;
        public uint[] ofsShaderBlocks;
        public ShaderBlock[] shaderBlocks;
    }

    public struct ShaderBlock
    {
        public ShaderBlockHeader header;
        public ShaderBlockHeader_GLSL3 GLSL3Header;
        public InputShaderInfo inputShaderInfo;
        public OutputShaderInfo outputShaderInfo;
        public UniformBufferInfo uniformBufferInfo;
        public SamplerShaderInfo sampleShaderInfo;
        public string shaderContent;
    }

    public struct ShaderBlockHeader
    {
        public uint flags;
        public uint flags2;
        public uint unk0;
        public uint unk1;
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint length;
    }

    public struct ShaderBlockHeader_GLSL3
    {
        public uint magic;
        public uint size;
        public uint type;
        public uint unk0;
        public uint target;
        public uint codeOffset;
        public uint codeSize;
        public uint unk1;
        public uint unk2; //-1
        public uint inputParamsOffset; //offset to array of input_shader_uniform_info_t
        public uint inputParamCount;
        public uint outputOffset; // offset to array of output_shader_uniform_info_t
        public uint outputCount;
        public uint uniformBufferOffset; // offset to array of uniformBuffer_shader_uniform_info_t
        public uint uniformBufferCount;
        public uint samplerUniformsOffset; //offset to sampler_shader_uniform_info_t
        public uint samplerUniformsCount;
        public uint unk3Offset;
        public uint unk3Count;
        public uint unk4Offset;
        public uint unk4Count;
        public uint variableStringsOffset;
        public uint variableStringsSize;
    }

    public struct InputShaderInfo
    {
        public uint glslParamNameOffset; //Offset to zero terminated string
        public uint unk0;
        public uint internalParamNameOffset;
        public uint unk1;
    }

    public struct OutputShaderInfo
    {
        public uint glslParamNameOffset; //Offset to zero terminated string
        public uint unk0;
        public uint internalParamNameOffset;
        public uint unk1;
    }

    public struct UniformBufferInfo
    {
        public uint glslParamNameOffset; //Offset to zero terminated string
        public uint unk0;
        public uint unk1;
    }

    public struct SamplerShaderInfo
    {
        public uint glslParamNameOffset; //Offset to zero terminated string
        public uint unk0;
        public uint unk1;
        public uint unk2;
    }
}
