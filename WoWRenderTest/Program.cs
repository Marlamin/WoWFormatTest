using System;
/*using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;*/
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.D3DCompiler;
using Resource = SharpDX.Direct3D11.Resource;
using Color = SharpDX.Color;

using System.IO;
using MapFlags = SharpDX.Direct3D11.MapFlags;
//using RecastManaged;

namespace WoWRenderTest
{
    static class Extensions
    {
        //static 
    }

    class Shader
    {
        public VertexShader VertexShader { get; private set; }
        public PixelShader PixelShader { get; private set; }
        public ShaderSignature Signature { get; private set; }
        public InputLayout Layout { get; private set; }

        public Shader(Device device, string shader, InputElement[] elements)
        {
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile(shader, "VS", "vs_4_0");
            VertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile(shader, "PS", "ps_4_0");
            PixelShader = new PixelShader(device, pixelShaderByteCode);

            Signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

            Layout = new InputLayout(device, Signature, elements);
        }
    }

    static class Program
    {
        private struct Constants
        {
            public Matrix m;
            public Vector4 c;
        }

        [STAThread]
        private static void Main()
        {
            RenderForm form = new RenderForm("Adt5")
            {
                Icon = SystemIcons.Application,
                WindowState = FormWindowState.Maximized
            };

            var desc = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device device;
            SwapChain swapChain;
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, desc, out device, out swapChain);
            var context = device.ImmediateContext;

            var constantBuffer = new Buffer(device, Utilities.SizeOf<Constants>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, Utilities.SizeOf<Constants>());

            var vertices = new Buffer(device, new BufferDescription(400000 * Utilities.SizeOf<Vector4>() * 2, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, Utilities.SizeOf<Vector4>() * 2));

            var indices = new Buffer(device, new BufferDescription(400000 * sizeof (int), ResourceUsage.Dynamic, BindFlags.IndexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, sizeof (int)));


            #region Shaders

            var shaders = new[]
            {
                new Shader(device, "Shaders/Color.fx", new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                }),
                new Shader(device, "Shaders/Texture.fx", new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
                    new InputElement("TEXCOORD", 1, Format.R32G32_Float, 24, 0)
                }),
                new Shader(device, "Shaders/Color.fx", new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 1)
                }),
                new Shader(device, "Shaders/Color.fx", new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 2)
                }),
                new Shader(device, "Shaders/Color.fx", new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 3)
                })
            };

            #endregion

            context.InputAssembler.InputLayout = shaders[1].Layout;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>() * 2, 0));
            //context.InputAssembler.SetIndexBuffer(indices, Format.R32_SInt, 0);
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            context.PixelShader.SetConstantBuffer(0, constantBuffer);
            context.VertexShader.Set(shaders[1].VertexShader);
            context.PixelShader.Set(shaders[1].PixelShader);

            DataStream vertexStream;
            context.MapSubresource(vertices, MapMode.WriteNoOverwrite, MapFlags.None, out vertexStream);

            var adt = new Adt(@"World\Maps\Azeroth\Azeroth_32_48.adt", device);

            vertexStream.WriteRange(adt.TerrainVerticesTextured);

            context.UnmapSubresource(vertices, 0);

            int pitch = 230;
            int yaw = 210;
            float distance = 400;
            bool textured = true;

            form.KeyDown += (target, arg) =>
            {
                switch (arg.KeyCode)
                {
                    case Keys.Up:
                        yaw += 5;
                        break;
                    case Keys.Down:
                        yaw -= 5;
                        break;
                    case Keys.Left:
                        pitch += 5;
                        break;
                    case Keys.Right:
                        pitch -= 5;
                        break;
                    case Keys.PageDown:
                        distance += 10;
                        break;
                    case Keys.PageUp:
                        distance -= 10;
                        break;
                    case Keys.F2:
                        if (textured)
                        {
                            vertices = new Buffer(device, new BufferDescription(400000 * Utilities.SizeOf<Vector4>() * 2, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, Utilities.SizeOf<Vector4>() * 2));
                            context.MapSubresource(vertices, MapMode.WriteNoOverwrite, MapFlags.None, out vertexStream);
                            vertexStream.WriteRange(adt.TerrainVertices);
                            context.UnmapSubresource(vertices, 0);
                        }
                        else
                        {
                            vertices = new Buffer(device, new BufferDescription(400000 * Utilities.SizeOf<Vector4>(), ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, Utilities.SizeOf<Vector4>()));
                            context.MapSubresource(vertices, MapMode.WriteNoOverwrite, MapFlags.None, out vertexStream);
                            vertexStream.WriteRange(adt.TerrainVerticesTextured);
                            context.UnmapSubresource(vertices, 0);
                        }
                        textured = !textured;
                        break;
                    case Keys.F3:
                        var fillmode = context.Rasterizer.State.Description.FillMode == FillMode.Solid ? FillMode.Wireframe : FillMode.Solid;
                        var tmp = context.Rasterizer.State.Description;
                        tmp.FillMode = fillmode;
                        context.Rasterizer.State = new RasterizerState(device, tmp);
                        break;
                    case Keys.F4:
                        /*
                        HeightField hf;
                        
                        float[] bmin = adt.boundingBox.Minimum.ToArray();
                        float[] bmax = adt.boundingBox.Maximum.ToArray();
                        
                        double TileSize = 1600.0 / 3;
                        int VoxelCount = 1000;

                        Config cfg = new Config()
                        {
                            bmin = bmin,
                            bmax = bmax,
                            ch = .1f,
                            cs = (float)(TileSize / VoxelCount),
                            walkableSlopeAngle = 45,
                            maxSimplificationError = 1.3f,
                            minRegionArea = 8*8,
                            mergeRegionArea = 20*20,
                            maxVertsPerPoly = 6,
                            tileSize = VoxelCount
                        };

                        Recast.CalcGridSize(bmin, bmax, cfg.cs, out cfg.width, out cfg.height);

                        cfg.walkableHeight = (int) Math.Ceiling(2.1 / cfg.ch);
                        cfg.walkableClimb = (int) Math.Floor(1.0 / cfg.ch);
                        cfg.walkableRadius = (int) Math.Ceiling(0.3 / cfg.cs);
                        cfg.maxEdgeLen = (int) (12 / cfg.cs);

                        bool result = Recast.CreateHeightfield(out hf, cfg.width, cfg.height, cfg.bmin, cfg.bmax, cfg.cs, cfg.ch);

                        var vertices2 = new List<float>();

                        foreach (var vertex in adt.TerrainVertices)
                        {
                            vertices2.AddRange(((Vector3) vertex).ToArray());
                        }

                        int count = adt.TerrainVertices.Length;
                        var indices2 = new List<int>();

                        for (int i = 0; i < count; i++)
                        {
                            indices2.Add(i + 1);
                        }

                        byte[] areas = new byte[indices2.Count / 3];
                        Recast.MarkWalkableTriangles(cfg.walkableSlopeAngle, vertices2.ToArray(), vertices2.Count / 3, indices2.ToArray(), indices2.Count / 3, out areas);
                        Recast.RasterizeTriangles(vertices2.ToArray(), vertices2.Count / 9, areas, hf);

                        Recast.FilterLowHangingWalkableObstacles(cfg.walkableClimb, hf);
                        Recast.FilterLedgeSpans(cfg.walkableHeight, cfg.walkableClimb, hf);
                        Recast.FilterWalkableLowHeightSpans(cfg.walkableHeight, hf);

                        CompactHeightField chf;
                        Recast.BuildCompactHeightfield(cfg.walkableHeight, cfg.walkableClimb, hf, out chf);
                        Recast.ErodeWalkableArea(cfg.walkableRadius, chf);
                        Recast.BuildDistanceField(chf);
                        Recast.BuildRegions(chf, cfg.borderSize, cfg.minRegionArea, cfg.mergeRegionArea);

                        ContourSet cset;
                        Recast.BuildContours(chf, cfg.maxSimplificationError, cfg.maxEdgeLen, out cset);
                        PolyMesh mesh;
                        Recast.BuildPolyMesh(cset, cfg.maxVertsPerPoly, out mesh);
                        PolyMeshDetail dmesh;
                        Recast.BuildPolyMeshDetail(mesh, chf, cfg.detailSampleDist, cfg.detailSampleMaxError, out dmesh);
                        */
                        break;
                         
                }

                distance = MathUtil.Clamp(distance, 100, 400);

                if (pitch < 0)
                    pitch += 360;

                pitch = pitch % 360;
                yaw = MathUtil.Clamp(yaw, 200, 260);
            };

            var origin = new Vector3((adt.X - 32) * 1600 / 3f + 1600 / 6f, 0, (32 - adt.Y) * 1600 / 3f - 1600 / 6f);

            Texture2D backBuffer = null;
            RenderTargetView renderView = null;
            Texture2D depthBuffer = null;
            DepthStencilView depthView = null;
            context.Rasterizer.SetViewport(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height));

            var resized = true;
            var proj = Matrix.Identity;

            context.Rasterizer.State = new RasterizerState(device, new RasterizerStateDescription
            {
                CullMode = CullMode.Back,
                DepthBias = 1,
                DepthBiasClamp = 10,
                FillMode = FillMode.Wireframe,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            });

            form.UserResized += (target, arg) => resized = true;
            Vector4 color = new Vector4(1, 1, 1, 1);

            RenderLoop.Run(form, () =>
            {
                if (resized)
                {

                    swapChain.ResizeBuffers(desc.BufferCount, form.ClientSize.Width, form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);
                    backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);

                    renderView = new RenderTargetView(device, backBuffer);

                    depthBuffer = new Texture2D(device, new Texture2DDescription
                    {
                        Format = Format.D32_Float_S8X24_UInt,
                        ArraySize = 1,
                        MipLevels = 0,
                        Width = form.ClientSize.Width,
                        Height = form.ClientSize.Height,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.DepthStencil,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });

                    depthView = new DepthStencilView(device, depthBuffer);
                    context.Rasterizer.SetViewport(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height));
                    proj = Matrix.PerspectiveFovLH((float)Math.PI / 2.0f, form.ClientSize.Width / (float)form.ClientSize.Height, 0.1f, 1000.0f);

                    context.OutputMerger.SetTargets(depthView, renderView);

                    resized = false;
                }

                var camera = origin;
                camera.X += distance;

                var q = Quaternion.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(pitch));

                var axis = Vector3.Transform(camera - origin, q);
                q = Quaternion.RotationAxis(Vector3.Cross(Vector3.UnitY, axis), MathUtil.DegreesToRadians(yaw));
                axis = Vector3.Transform(axis, q);

                var view = Matrix.LookAtLH(axis + origin, origin, Vector3.UnitY);
                var viewProj = Matrix.Multiply(view, proj);
                var worldViewProj = viewProj;
                worldViewProj.Transpose();

                Constants constants;
                constants.m = worldViewProj;
                constants.c = color;

                context.UpdateSubresource(ref constants, constantBuffer);

                form.Text = "Origin: " + origin + ", Distance: " + (camera - origin).Length() + ", Pitch: " + pitch + ", Yaw: " + yaw;

                context.ClearRenderTargetView(renderView, Color4.Black);
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);


                if (textured)
                {
                    context.InputAssembler.InputLayout = shaders[1].Layout;
                    context.VertexShader.Set(shaders[1].VertexShader);
                    context.PixelShader.Set(shaders[1].PixelShader);
                }
                else
                {
                    constants.c = new Vector4(0, 1, 0, 1);
                    context.UpdateSubresource(ref constants, constantBuffer);

                    context.InputAssembler.InputLayout = shaders[0].Layout;
                    context.VertexShader.Set(shaders[0].VertexShader);
                    context.PixelShader.Set(shaders[0].PixelShader);
                }

                foreach (var tile in adt.hora._info.MapChunks)
                {
                    tile.Render(device);
                }

                //Models

                constants.c = new Vector4(1, 0, 0, 1);
                context.UpdateSubresource(ref constants, constantBuffer);

                context.InputAssembler.InputLayout = shaders[2].Layout;
                context.VertexShader.Set(shaders[2].VertexShader);
                context.PixelShader.Set(shaders[2].PixelShader);

                foreach (var model in adt.hora.adtmodels)
                {
                    model.Render(device);
                }

                //Wmo

                constants.c = new Vector4(1, 1, 0, 1);
                context.UpdateSubresource(ref constants, constantBuffer);

                context.InputAssembler.InputLayout = shaders[3].Layout;
                context.VertexShader.Set(shaders[3].VertexShader);
                context.PixelShader.Set(shaders[3].PixelShader);

                foreach (var model in adt.hora.wmo_models)
                {
                    model.Render(device);
                }

                //Water

                constants.c = new Vector4(0, 0, 1, 1);
                context.UpdateSubresource(ref constants, constantBuffer);

                context.InputAssembler.InputLayout = shaders[4].Layout;
                context.VertexShader.Set(shaders[4].VertexShader);
                context.PixelShader.Set(shaders[4].PixelShader);

                device.ImmediateContext.Draw(adt.hora.waterverticescount, 0);

                swapChain.Present(0, PresentFlags.None);
            });
        }
    }
}