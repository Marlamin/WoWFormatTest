using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace WoWFormatUI
{
    public class Render : D3D11
    {
        public int indicecount;

        private ConstantBuffer<Projections> m_pConstantBuffer;

        private PixelShader m_pPixelShader;

        private VertexShader m_pVertexShader;
        
        public Render()
        {
            Go(@"World\ArtTest\Boxtest\xyz.m2");
        }

        public Render(string ModelPath)
        {
            Go(ModelPath);
        }

        private void Go(string ModelPath)
        {
            using (var dg = new DisposeGroup())
            {
                //Load Shaders
                var pVSBlob = dg.Add(ShaderBytecode.CompileFromFile("RenderWithCam.fx", "VS", "vs_4_0"));
                var inputSignature = dg.Add(ShaderSignature.GetInputSignature(pVSBlob));
                m_pVertexShader = new VertexShader(Device, pVSBlob);

                var pPSBlob = dg.Add(ShaderBytecode.CompileFromFile("RenderWithCam.fx", "PS", "ps_4_0"));
                m_pPixelShader = new PixelShader(Device, pPSBlob);

                //Define layout
                var layout = dg.Add(new InputLayout(Device, inputSignature, new[]{
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0)
                }));

                //Load model
                M2Reader reader = new M2Reader("Z:\\18566_full\\");
                string filename = ModelPath;
                reader.LoadM2(filename);

                //Load vertices
                List<float> verticelist = new List<float>();
                for (int i = 0; i < reader.model.vertices.Count(); i++)
                {
                    verticelist.Add(reader.model.vertices[i].position.X);
                    verticelist.Add(reader.model.vertices[i].position.Z * -1);
                    verticelist.Add(reader.model.vertices[i].position.Y);
                    verticelist.Add(1.0f);
                    verticelist.Add(reader.model.vertices[i].textureCoordX);
                    verticelist.Add(reader.model.vertices[i].textureCoordY);
                }

                //Load indices
                List<ushort> indicelist = new List<ushort>();
                for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
                {
                    indicelist.Add(reader.model.skins[0].triangles[i].pt1);
                    indicelist.Add(reader.model.skins[0].triangles[i].pt2);
                    indicelist.Add(reader.model.skins[0].triangles[i].pt3);
                }

                //Convert to array
                ushort[] indices = indicelist.ToArray();
                float[] vertices = verticelist.ToArray();

                //Set count for use in draw later on
                indicecount = indices.Count();

                //Create buffers
                var vertexBuffer = dg.Add(Buffer.Create(Device, BindFlags.VertexBuffer, vertices));
                var vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>() + Utilities.SizeOf<Vector2>(), 0);
                var indexBuffer = dg.Add(Buffer.Create(Device, BindFlags.IndexBuffer, indices));

                Device.ImmediateContext.InputAssembler.InputLayout = (layout);
                Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
                Device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
                Device.ImmediateContext.InputAssembler.PrimitiveTopology = (PrimitiveTopology.TriangleList);

                //Get texture, what a mess this could be much better
                var blp = new BLPReader("Z:\\18566_full\\");
                if (File.Exists(Path.Combine("Z:\\18566_full\\", reader.model.filename.Replace("M2", "blp"))))
                {
                    blp.LoadBLP(reader.model.filename.Replace("M2", "blp"));
                }
                else
                {
                    if (reader.model.textures.Count() > 0)
                    {
                        blp.LoadBLP(reader.model.textures[0]);
                    }
                    else
                    {
                        throw new Exception("No forking textures, mate.");
                    }
                }

                MemoryStream s = new MemoryStream();
                blp.bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                s.Seek(0, SeekOrigin.Begin);
                Texture2D texture = Texture2D.FromMemory<Texture2D>(Device, s.ToArray());

                var textureView = new ShaderResourceView(Device, texture);

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
                //End of texture stuff,

                Set(ref m_pConstantBuffer, new ConstantBuffer<Projections>(Device));
                Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_pConstantBuffer.Buffer);
            }

            //Make camera
            Camera = new FirstPersonCamera();
            Camera.SetProjParams((float)Math.PI / 2, 1, 0.01f, 100.0f);
            Camera.SetViewParams(new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0.0f, 1.0f, 0.0f));
        }

        public override void RenderScene(DrawEventArgs args)
        {
            float t = 1f;

            Device.ImmediateContext.ClearRenderTargetView(this.RenderTargetView, Color.White);
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
            Device.ImmediateContext.DrawIndexed(indicecount, 0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Set(ref m_pVertexShader, null);
            Set(ref m_pPixelShader, null);
            Set(ref m_pConstantBuffer, null);
        }
    }
}