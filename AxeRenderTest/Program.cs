using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;
using DriverType = SharpDX.Direct3D10.DriverType;
using WoWFormatLib;
using WoWFormatLib.FileReaders;
using SharpDX.DirectInput;

namespace AxeRenderTest
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var form = new RenderForm("Axe Render Test HYPE");
            form.Width = 1280;
            form.Height = 720;

            // SwapChain description
            var desc = new SwapChainDescription()
                           {
                               BufferCount = 1,
                               ModeDescription= 
                                   new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                                       new Rational(60, 1), Format.R8G8B8A8_UNorm),
                               IsWindowed = true,
                               OutputHandle = form.Handle,
                               SampleDescription = new SampleDescription(1, 0),
                               SwapEffect = SwapEffect.Discard,
                               Usage = Usage.RenderTargetOutput
                           };

            //Load M2
            M2Reader reader = new M2Reader("Z:\\18566_full\\");
            reader.LoadM2(@"Item\Objectcomponents\weapon\axe_1h_blacksmithing_d_01.M2");
            
            // Create Device and SwapChain
            Device device;
            SwapChain swapChain;
            
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
            var context = device;

            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            var renderView = new RenderTargetView(device, backBuffer);

            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("RenderTest.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("RenderTest.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(device, pixelShaderByteCode);

            // Layout from VertexShader input signature
            var layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                    });

            // Instantiate Vertex buiffer from vertex data
            var data = new Vector4[reader.model.vertices.Count()];
            for (int i = 0; i < reader.model.vertices.Count(); i++)
            {
                data[i] = new Vector4(reader.model.vertices[i].position.X, reader.model.vertices[i].position.Z * -1, -reader.model.vertices[i].position.Y, 1.0f);
            }

            List<int> list = new List<int>();
            for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                list.Add(reader.model.skins[0].triangles[i].pt1);
                list.Add(reader.model.skins[0].triangles[i].pt2);
                list.Add(reader.model.skins[0].triangles[i].pt3);
            }

            int[] indices = list.ToArray();

            var indexBuffer = SharpDX.Direct3D10.Buffer.Create(device, BindFlags.IndexBuffer, indices);
            var vertices = Buffer.Create(device, BindFlags.VertexBuffer, data);
            var vertexBufferBinding = new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>(), 0);

            // Create Constant Buffer
            var contantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None);
            
            // Create Depth Buffer & View
            var depthBuffer = new Texture2D(device, new Texture2DDescription()
                    {
                        Format = Format.D32_Float_S8X24_UInt,
                        ArraySize = 1,
                        MipLevels = 1,
                        Width = form.ClientSize.Width,
                        Height = form.ClientSize.Height,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.DepthStencil,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });

            var depthView = new DepthStencilView(device, depthBuffer);

            // Prepare All the stages
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

            context.VertexShader.Set(vertexShader);
            context.Rasterizer.SetViewports(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));
            context.PixelShader.Set(pixelShader);

            // Prepare matrices
            var view = Matrix.LookAtLH(new Vector3(0, 0, -2), new Vector3(0, 0, 0), Vector3.UnitY);
            var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, form.ClientSize.Width / (float)form.ClientSize.Height, 0.1f, 100.0f);
            var viewProj = Matrix.Multiply(view, proj);

            // Initialize DirectInput
            var directInput = new DirectInput();

            // Instantiate the keyboard
            var keyboard = new Keyboard(directInput);

            // Acquire the keyboard
            keyboard.Properties.BufferSize = 128;

            keyboard.Acquire();

            float camx = 0;
            float camy = 0;
            float camz = 0;
            // Main loop
            RenderLoop.Run(form, () =>
                {
                    keyboard.Poll();
                    var datas = keyboard.GetBufferedData();

                    foreach (KeyboardUpdate update in datas)
                    {
                        if (update.Key == Key.W)
                        {
                            camz = camz + .025f;
                        }
                        if (update.Key == Key.D)
                        {
                            camy = camy + .025f;
                        }
                        if (update.Key == Key.A)
                        {
                            camy = camy - .025f;
                        }
                        if (update.Key == Key.S)
                        {
                            camz = camz - .025f;
                        }
                    }
                    context.OutputMerger.SetTargets(depthView, renderView);

                    context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                    context.ClearRenderTargetView(renderView, Color.White);

                    var worldViewProj = Matrix.RotationX(camx) * Matrix.RotationY(camy) * Matrix.RotationZ(camz * .2f) * viewProj;
                    worldViewProj.Transpose();
                    context.UpdateSubresource(ref worldViewProj, contantBuffer);

                    context.DrawIndexed(indices.Count(), 0, 0);
                    swapChain.Present(0, PresentFlags.None);
                });

            // Release all resources
            vertexShaderByteCode.Dispose();
            vertexShader.Dispose();
            pixelShaderByteCode.Dispose();
            pixelShader.Dispose();
            vertices.Dispose();
            layout.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();
            context.ClearState();
            context.Flush();
            device.Dispose();
            context.Dispose();
            swapChain.Dispose();
            factory.Dispose();
        }
    }
}