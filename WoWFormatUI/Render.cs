using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WPF;
using SharpDX.WPF.Cameras;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Structs.M2;
using WoWRenderLib;
using WoWRenderLib.Cameras;
using WoWRenderLib.Structs.WoWM2;
using WoWRenderLib.Structs.WoWWMO;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace WoWFormatUI
{
    public class Render : D3D11
    {
        private int indicecount;
        private ConstantBuffer<Projections> m_pConstantBuffer;
        private PixelShader m_pPixelShader;
        private VertexShader m_pVertexShader;
        private M2Material[] m2materials;
        private M2RenderBatch[] M2renderBatches;
        private WMOMaterial[] materials;
        private bool modelLoaded = false;
        private WMORenderBatch[] WMOrenderBatches;

        public Render()
        {
            //RenderModel(@"World\ArtTest\Boxtest\xyz.m2");
        }

        public Render(string ModelPath)
        {
            if (ModelPath.EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
            {
                RenderM2(ModelPath);
                modelLoaded = true;
            }
            else if (ModelPath.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase))
            {
                RenderWMO(ModelPath);
                modelLoaded = true;
            }
        }

        public override void RenderScene(DrawEventArgs args)
        {
            if (!modelLoaded)
                return;

            float t = 1f;

            Device.ImmediateContext.ClearRenderTargetView(this.RenderTargetView, SharpDX.Color.White);
            Device.ImmediateContext.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            var matWorld = Matrix.RotationX(t);

            m_pConstantBuffer.Value = new Projections
            {
                Projection = Matrix.Transpose(Camera.Projection),
                View = Matrix.Transpose(Camera.View),
                World = Matrix.Transpose(matWorld),
            };

            Device.ImmediateContext.VertexShader.Set(m_pVertexShader);
            Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_pConstantBuffer.Buffer);
            Device.ImmediateContext.PixelShader.Set(m_pPixelShader);
            Device.ImmediateContext.PixelShader.SetConstantBuffer(0, m_pConstantBuffer.Buffer);

            if (WMOrenderBatches != null)
            {
                for (int i = 0; i < WMOrenderBatches.Count(); i++)
                {
                    var textureView = new ShaderResourceView(Device, materials[WMOrenderBatches[i].materialID].texture);
                    var sampler = new SamplerState(Device, new SamplerStateDescription()
                    {
                        Filter = Filter.MinMagMipLinear,
                        AddressU = TextureAddressMode.Wrap,
                        AddressV = TextureAddressMode.Wrap,
                        AddressW = TextureAddressMode.Wrap,
                        BorderColor = SharpDX.Color.Black,
                        ComparisonFunction = Comparison.Never,
                        MaximumAnisotropy = 16,
                        MipLodBias = 0,
                        MinimumLod = 0,
                        MaximumLod = 16,
                    });

                    Device.ImmediateContext.PixelShader.SetSampler(0, sampler);
                    Device.ImmediateContext.PixelShader.SetShaderResource(0, textureView);

                    Device.ImmediateContext.DrawIndexed((int)WMOrenderBatches[i].numFaces, (int)WMOrenderBatches[i].firstFace, 0);
                }
            }
            /*else
            {
                Device.ImmediateContext.DrawIndexed(indicecount, 0, 0);
            }*/
            if (M2renderBatches != null)
            {
                for (int i = 0; i < M2renderBatches.Count(); i++)
                {
                    if (M2renderBatches[i].materialID > m2materials.Count())
                    {
                        continue;
                    }
                    var textureView = new ShaderResourceView(Device, m2materials[M2renderBatches[i].materialID].texture);
                    var sampler = new SamplerState(Device, new SamplerStateDescription()
                    {
                        Filter = Filter.MinMagMipLinear,
                        AddressU = TextureAddressMode.Wrap,
                        AddressV = TextureAddressMode.Wrap,
                        AddressW = TextureAddressMode.Wrap,
                        BorderColor = SharpDX.Color.Black,
                        ComparisonFunction = Comparison.Never,
                        MaximumAnisotropy = 16,
                        MipLodBias = 0,
                        MinimumLod = 0,
                        MaximumLod = 16,
                    });

                    Device.ImmediateContext.PixelShader.SetSampler(0, sampler);
                    Device.ImmediateContext.PixelShader.SetShaderResource(0, textureView);

                    Device.ImmediateContext.DrawIndexed((int)M2renderBatches[i].numFaces, (int)M2renderBatches[i].firstFace, 0);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Set(ref m_pVertexShader, null);
            Set(ref m_pPixelShader, null);
            Set(ref m_pConstantBuffer, null);
        }

        private void RenderM2(string ModelPath)
        {
            using (var dg = new DisposeGroup())
            {
                string _BaseDir = ConfigurationManager.AppSettings["basedir"];

                //Load Shaders
                var pVSBlob = dg.Add(ShaderBytecode.CompileFromFile("RenderWithCam.fx", "VS", "vs_4_0"));
                var inputSignature = dg.Add(ShaderSignature.GetInputSignature(pVSBlob));
                m_pVertexShader = new VertexShader(Device, pVSBlob);

                var pPSBlob = dg.Add(ShaderBytecode.CompileFromFile("RenderWithCam.fx", "PS", "ps_4_0"));
                m_pPixelShader = new PixelShader(Device, pPSBlob);

                //Define layout
                var layout = dg.Add(new InputLayout(Device, inputSignature, new[]{
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 16, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 28, 0)
                }));

                M2Loader model = new M2Loader(_BaseDir, ModelPath, Device);
                model.LoadM2();
                WoWM2 m2 = model.m2;

                //Set count for use in draw later on
                indicecount = m2.indices.Count();

                //Create buffers
                var vertexBuffer = dg.Add(Buffer.Create(Device, BindFlags.VertexBuffer, m2.vertices));
                var vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>() + Utilities.SizeOf<Vector3>() + Utilities.SizeOf<Vector2>(), 0);
                var indexBuffer = dg.Add(Buffer.Create(Device, BindFlags.IndexBuffer, m2.indices));

                Device.ImmediateContext.InputAssembler.InputLayout = (layout);
                Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
                Device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
                Device.ImmediateContext.InputAssembler.PrimitiveTopology = (PrimitiveTopology.TriangleList);

                M2renderBatches = m2.renderBatches;
                m2materials = m2.materials;

                Set(ref m_pConstantBuffer, new ConstantBuffer<Projections>(Device));
                Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_pConstantBuffer.Buffer);
            }

            //Make camera
            Camera = new WoWModelViewerCamera();
            Camera.SetProjParams((float)Math.PI / 2, 1, 0.01f, 100.0f);
            Camera.SetViewParams(new Vector3(10.0f, 10.0f, -10.0f), new Vector3(0.0f, 1.0f, 0.0f));
            Camera.Roll(-4f);
        }

        private void RenderWMO(string ModelPath)
        {
            using (var dg = new DisposeGroup())
            {
                string _BaseDir = ConfigurationManager.AppSettings["basedir"];

                //Load Shaders
                var pVSBlob = dg.Add(ShaderBytecode.CompileFromFile("RenderWithCam.fx", "VS", "vs_4_0"));
                var inputSignature = dg.Add(ShaderSignature.GetInputSignature(pVSBlob));
                m_pVertexShader = new VertexShader(Device, pVSBlob);

                var pPSBlob = dg.Add(ShaderBytecode.CompileFromFile("RenderWithCam.fx", "PS", "ps_4_0"));
                m_pPixelShader = new PixelShader(Device, pPSBlob);

                //Define layout
                var layout = dg.Add(new InputLayout(Device, inputSignature, new[]{
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 16, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 28, 0)
                }));

                WMOLoader model = new WMOLoader(_BaseDir, ModelPath, Device);
                model.LoadWMO();
                WoWWMO wmo = model.wmo;

                indicecount = wmo.indices.Count();

                var vertexBuffer = dg.Add(Buffer.Create(Device, BindFlags.VertexBuffer, wmo.vertices));
                var vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>() + Utilities.SizeOf<Vector3>() + Utilities.SizeOf<Vector2>(), 0);
                var indexBuffer = dg.Add(Buffer.Create(Device, BindFlags.IndexBuffer, wmo.indices));

                Device.ImmediateContext.InputAssembler.InputLayout = (layout);
                Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
                Device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
                Device.ImmediateContext.InputAssembler.PrimitiveTopology = (PrimitiveTopology.TriangleList);

                WMOrenderBatches = wmo.renderBatches;
                materials = wmo.materials;

                Set(ref m_pConstantBuffer, new ConstantBuffer<Projections>(Device));
                Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_pConstantBuffer.Buffer);
            }

            //Make camera
            Camera = new WoWModelViewerCamera();
            Camera.SetProjParams((float)Math.PI / 2, 1, 0.01f, 100.0f);
            Camera.SetViewParams(new Vector3(10.0f, 10.0f, -10.0f), new Vector3(0.0f, 1.0f, 0.0f));
            Camera.Roll(-4f);
        }
    }
}