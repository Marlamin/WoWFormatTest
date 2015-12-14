using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;
using CASCExplorer;

namespace OBJExporterUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string outdir;
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly BackgroundWorker exportworker = new BackgroundWorker();
        private readonly BackgroundWorkerEx cascworker = new BackgroundWorkerEx();

        private bool showADT = true;
        private bool showM2 = false;
        private bool showWMO = true;

        private List<String> files;

        public MainWindow()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["firstrun"]) == true)
            {
                var cfgWindow = new ConfigurationWindow();
                cfgWindow.ShowDialog();
            }

            InitializeComponent();

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;

            exportworker.DoWork += exportworker_DoWork;
            exportworker.RunWorkerCompleted += exportworker_RunWorkerCompleted;
            exportworker.ProgressChanged += worker_ProgressChanged;
            exportworker.WorkerReportsProgress = true;

            cascworker.DoWork += cascworker_DoWork;
            cascworker.RunWorkerCompleted += cascworker_RunWorkerCompleted;
            cascworker.ProgressChanged += worker_ProgressChanged;
            cascworker.WorkerReportsProgress = true;
        }

        private void cascworker_DoWork(object sender, DoWorkEventArgs e)
        {
            var basedir = ConfigurationManager.AppSettings["basedir"];
            if (Directory.Exists(basedir))
            {
                if (File.Exists(Path.Combine(basedir, ".build.info")))
                {
                    cascworker.ReportProgress(0, "Loading WoW from disk..");
                    CASC.InitCasc(cascworker, basedir, ConfigurationManager.AppSettings["program"]);
                }
                else
                {
                    throw new Exception("Unable to find World of Warcraft client!");
                }
            }
            else
            {
                cascworker.ReportProgress(0, "Loading WoW from web..");
                CASC.InitCasc(cascworker, null, ConfigurationManager.AppSettings["program"]);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Content = "";
            loadingLabel.Visibility = Visibility.Visible;
            adtCheckBox.IsEnabled = false;
            wmoCheckBox.IsEnabled = false;
            m2CheckBox.IsEnabled = false;
            buildsBox.IsEnabled = false;
            exportButton.IsEnabled = false;
            buildsBox.Visibility = Visibility.Hidden; // Hide for now, overlap with progress bar
            exportworker.RunWorkerAsync(modelListBox.SelectedValue);
        }

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<String> filtered = new List<String>();

            for (int i = 0; i < files.Count(); i++)
            {
                if (files[i].IndexOf(filterTextBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    filtered.Add(files[i]);
                }
            }

            modelListBox.DataContext = filtered;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            outdir = ConfigurationManager.AppSettings["outdir"];

            cascworker.RunWorkerAsync();
        }

        private void cascworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            files = new List<String>();

            loadingImage.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Visible;

            worker.RunWorkerAsync();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Visibility = Visibility.Hidden;
            loadingLabel.Visibility = Visibility.Hidden;
            modelListBox.Visibility = Visibility.Visible;
            filterTextBox.Visibility = Visibility.Visible;
            exportButton.Visibility = Visibility.Visible;
            wmoCheckBox.Visibility = Visibility.Visible;
            m2CheckBox.Visibility = Visibility.Visible;
            adtCheckBox.Visibility = Visibility.Visible;
            //buildsBox.Visibility = Visibility.Visible;

            modelListBox.DataContext = files;
        }

        private void exportworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportButton.IsEnabled = true;
            progressBar.Visibility = Visibility.Hidden;
            loadingLabel.Visibility = Visibility.Hidden;
            adtCheckBox.IsEnabled = true;
            wmoCheckBox.IsEnabled = true;
            m2CheckBox.IsEnabled = true;
            //buildsBox.Visibility = Visibility.Visible;
            buildsBox.IsEnabled = true;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string)e.UserState;

            if (!string.IsNullOrEmpty(state))
            {
                loadingLabel.Content = state;
            }
            
            progressBar.Value = e.ProgressPercentage;
        }

        private void exportworker_DoWork(object sender, DoWorkEventArgs e)
        {
            string file = (string) e.Argument;

            if (!CASC.FileExists(file)) { return; }
            if (file.EndsWith(".wmo")){
                WMOtoOBJ(file);
            }
            else if(file.EndsWith(".m2")){
                M2toOBJ(file);
            }
            else if (file.EndsWith(".adt"))
            {
                ADTtoOBJ(file);
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            if (!File.Exists("listfile.txt"))
            {
                throw new Exception("Listfile not found. Unable to continue.");
            }

            string[] lines = File.ReadAllLines("listfile.txt");
            
            for(int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].ToLower();
            }

            List<string> unwantedExtensions = new List<String>();
            for (int u = 0; u < 512; u++)
            {
                unwantedExtensions.Add("_" + u.ToString().PadLeft(3, '0') + ".wmo");
            }

            string[] unwanted = unwantedExtensions.ToArray();

            for (int i = 0; i < lines.Count(); i++)
            {
                if (!CASC.FileExists(lines[i])) { continue; }

                if (showADT && lines[i].EndsWith(".adt")) {
                    if(!lines[i].EndsWith("obj0.adt") && !lines[i].EndsWith("obj1.adt") && !lines[i].EndsWith("tex0.adt") && !lines[i].EndsWith("tex1.adt") && !lines[i].EndsWith("_lod.adt"))
                    {
                        if (!files.Contains(lines[i])) { files.Add(lines[i]); }
                    }
                }

                if (showWMO && lines[i].EndsWith(".wmo")) {
                    if (!unwanted.Contains(lines[i].Substring(lines[i].Length - 8, 8)) && !lines[i].EndsWith("lod.wmo")) {
                        if (!files.Contains(lines[i])) { files.Add(lines[i]); }
                    }
                }

                if (showM2 && lines[i].EndsWith(".m2")) {
                    if (!lines[i].StartsWith("alternate") && !lines[i].StartsWith("camera") && !lines[i].StartsWith("spells")) {
                        if (!files.Contains(lines[i])) { files.Add(lines[i]); }
                    }
                }

                if (i % 100 == 0)
                {
                    var progress = (i * 100) / lines.Count();
                    worker.ReportProgress(progress, "Loading listfile..");
                }
            }
        }

        public void M2toOBJ(string file)
        {
            var outdir = ConfigurationManager.AppSettings["outdir"];
            var reader = new M2Reader();

            exportworker.ReportProgress(15, "Reading M2..");

            if (!CASC.FileExists(file)) { throw new Exception("404 M2 not found!"); }

            reader.LoadM2(file);

            Vertex[] vertices = new Vertex[reader.model.vertices.Count()];

            for (int i = 0; i < reader.model.vertices.Count(); i++)
            {
                vertices[i].Position = new Vector3(reader.model.vertices[i].position.X, reader.model.vertices[i].position.Z, reader.model.vertices[i].position.Y);
                vertices[i].Normal = new Vector3(reader.model.vertices[i].normal.X, reader.model.vertices[i].normal.Z, reader.model.vertices[i].normal.Y);
                vertices[i].TexCoord = new Vector3(reader.model.vertices[i].textureCoordX, reader.model.vertices[i].textureCoordY, (float)0.0);
            }

            // Create output directory
            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            var objsw = new StreamWriter(Path.Combine(outdir, file.Replace(".m2", ".obj")));

            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file) + ".mtl");

            foreach (var vertex in vertices)
            {
                objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                objsw.WriteLine("vn " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Normal.Z);
            }

            List<uint> indicelist = new List<uint>();
            for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                var t = reader.model.skins[0].triangles[i];
                indicelist.Add(t.pt1);
                indicelist.Add(t.pt2);
                indicelist.Add(t.pt3);
            }

            var indices = indicelist.ToArray();
            exportworker.ReportProgress(35, "Writing files..");

            var renderbatches = new RenderBatch[reader.model.skins[0].submeshes.Count()];
            for (int i = 0; i < reader.model.skins[0].submeshes.Count(); i++)
            {
                if (file.StartsWith("character", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (reader.model.skins[0].submeshes[i].submeshID != 0)
                    {
                        if (!reader.model.skins[0].submeshes[i].submeshID.ToString().EndsWith("01"))
                        {
                            continue;
                        }
                    }
                }

                renderbatches[i].firstFace = reader.model.skins[0].submeshes[i].startTriangle;
                renderbatches[i].numFaces = reader.model.skins[0].submeshes[i].nTriangles;
                renderbatches[i].groupID = (uint)i;
                for (int tu = 0; tu < reader.model.skins[0].textureunit.Count(); tu++)
                {
                    if (reader.model.skins[0].textureunit[tu].submeshIndex == i)
                    {
                        renderbatches[i].blendType = reader.model.renderflags[reader.model.skins[0].textureunit[tu].renderFlags].blendingMode;
                        renderbatches[i].materialID = reader.model.texlookup[reader.model.skins[0].textureunit[tu].texture].textureID;
                    }
                }
            }

            exportworker.ReportProgress(65, "Exporting textures..");

            var mtlsb = new StringBuilder();
            var textureID = 0;
            var materials = new Material[reader.model.textures.Count()];

            for (int i = 0; i < reader.model.textures.Count(); i++)
            {
                string texturefilename = "Dungeons\\Textures\\testing\\COLOR_13.blp";
                materials[i].flags = reader.model.textures[i].flags;
                switch (reader.model.textures[i].type)
                {
                    case 0:
                        //Console.WriteLine("      Texture given in file!");
                        texturefilename = reader.model.textures[i].filename;
                        break;
                    case 1:
                        string[] csfilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(file, (int)reader.model.textures[i].type, i);
                        if (csfilenames.Count() > 0)
                        {
                            texturefilename = csfilenames[0];
                        }
                        else
                        {
                            //Console.WriteLine("      No type 1 texture found, falling back to placeholder texture");
                        }
                        break;
                    case 2:
                        if (WoWFormatLib.Utils.CASC.FileExists(Path.ChangeExtension(file, ".blp")))
                        {
                            //Console.WriteLine("      BLP exists!");
                            texturefilename = Path.ChangeExtension(file, ".blp");
                        }
                        else
                        {
                            //Console.WriteLine("      Type 2 does not exist!");
                            //needs lookup?
                        }
                        break;
                    case 11:
                        string[] cdifilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(file, (int)reader.model.textures[i].type);
                        for (int ti = 0; ti < cdifilenames.Count(); ti++)
                        {
                            if (WoWFormatLib.Utils.CASC.FileExists(file.Replace(reader.model.name + ".M2", cdifilenames[ti] + ".blp")))
                            {
                                texturefilename = file.Replace(reader.model.name + ".M2", cdifilenames[ti] + ".blp");
                            }
                        }
                        break;
                    default:
                       // Console.WriteLine("      Falling back to placeholder texture");
                        break;
                }

                //Console.WriteLine("      Eventual filename is " + texturefilename);

                materials[i].textureID = textureID + i;
                materials[i].filename = Path.GetFileNameWithoutExtension(texturefilename);

                var blpreader = new BLPReader();

                blpreader.LoadBLP(texturefilename);

                try
                {
                    blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file), materials[i].filename + ".png"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            exportworker.ReportProgress(85, "Writing files..");

            foreach (var material in materials)
            {
                mtlsb.Append("newmtl " + material.filename);
                mtlsb.Append("illum 2");
                mtlsb.Append("map_Ka " + material.filename + ".png");
                mtlsb.Append("map_Kd " + material.filename + ".png");
            }

            File.WriteAllText(Path.Combine(outdir, file.Replace(".m2", ".mtl")), mtlsb.ToString());

            objsw.WriteLine("g " + Path.GetFileNameWithoutExtension(file));

            foreach (var renderbatch in renderbatches)
            {
                var i = renderbatch.firstFace;
                objsw.WriteLine("o " + Path.GetFileNameWithoutExtension(file) + renderbatch.groupID);
                objsw.WriteLine("usemtl " + materials[renderbatch.materialID].filename);
                objsw.WriteLine("s 1");
                while (i < (renderbatch.firstFace + renderbatch.numFaces))
                {
                    objsw.WriteLine("f " + (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1) + " " + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1));
                    i = i + 3;
                }
            }

            objsw.Close();
            // https://en.wikipedia.org/wiki/Wavefront_.obj_file#Basic_materials
            // http://wiki.unity3d.com/index.php?title=ExportOBJ
            // http://web.cse.ohio-state.edu/~hwshen/581/Site/Lab3_files/Labhelp_Obj_parser.htm

            Console.WriteLine("Done loading model!");
        }

        public void WMOtoOBJ(string file)
        {
            Console.WriteLine("Loading WMO file..");

            exportworker.ReportProgress(5, "Reading WMO..");

            var outdir = ConfigurationManager.AppSettings["outdir"];
            WMOReader reader = new WMOReader();
            reader.LoadWMO(file);

            // TODO: Support doodads!
            for (int i = 0; i < reader.wmofile.doodadNames.Count(); i++)
            {
                //Console.WriteLine(reader.wmofile.doodadNames[i].filename);
                //reader.wmofile.doodadDefinitions[i].
                //reader.wmofile.doodadDefinitions[i].
            }

            exportworker.ReportProgress(30, "Reading WMO..");

            uint totalVertices = 0;

            var groups = new WMOGroup[reader.wmofile.group.Count()];

            for (int g = 0; g < reader.wmofile.group.Count(); g++)
            {
                if (reader.wmofile.group[g].mogp.vertices == null) { continue; }
                for (int i = 0; i < reader.wmofile.groupNames.Count(); i++)
                {
                    if (reader.wmofile.group[g].mogp.nameOffset == reader.wmofile.groupNames[i].offset)
                    {
                        groups[g].name = reader.wmofile.groupNames[i].name.Replace(" ", "_");
                    }
                }

                if(groups[g].name == "antiportal") { continue; }

                groups[g].verticeOffset = totalVertices;
                groups[g].vertices = new Vertex[reader.wmofile.group[g].mogp.vertices.Count()];

                for (int i = 0; i < reader.wmofile.group[g].mogp.vertices.Count(); i++)
                {
                    groups[g].vertices[i].Position = new Vector3(reader.wmofile.group[g].mogp.vertices[i].vector.X * -1, reader.wmofile.group[g].mogp.vertices[i].vector.Z, reader.wmofile.group[g].mogp.vertices[i].vector.Y);
                    groups[g].vertices[i].Normal = new Vector3(reader.wmofile.group[g].mogp.normals[i].normal.X, reader.wmofile.group[g].mogp.normals[i].normal.Z, reader.wmofile.group[g].mogp.normals[i].normal.Y);
                    groups[g].vertices[i].TexCoord = new Vector3(reader.wmofile.group[g].mogp.textureCoords[0][i].X, reader.wmofile.group[g].mogp.textureCoords[0][i].Y, 0.0f);
                    totalVertices++;
                }

                var indicelist = new List<uint>();

                for (int i = 0; i < reader.wmofile.group[g].mogp.indices.Count(); i++)
                {
                    indicelist.Add(reader.wmofile.group[g].mogp.indices[i].indice);
                }

                groups[g].indices = indicelist.ToArray();
            }

            exportworker.ReportProgress(55, "Exporting textures..");

            // Create output directory
            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            var mtlsb = new StringBuilder();
            var textureID = 0;

            var materials = new Material[reader.wmofile.materials.Count()];
            for (int i = 0; i < reader.wmofile.materials.Count(); i++)
            {
                for (int ti = 0; ti < reader.wmofile.textures.Count(); ti++)
                {
                    if (reader.wmofile.textures[ti].startOffset == reader.wmofile.materials[i].texture1)
                    {
                        //materials[i].textureID = BLPLoader.LoadTexture(reader.wmofile.textures[ti].filename, cache);
                        materials[i].textureID = textureID + i;
                        materials[i].filename = Path.GetFileNameWithoutExtension(reader.wmofile.textures[ti].filename);
                        if (reader.wmofile.materials[i].blendMode == 0)
                        {
                            materials[i].transparent = false;
                        }
                        else
                        {
                            materials[i].transparent = true;
                        }

                        var blpreader = new BLPReader();

                        blpreader.LoadBLP(reader.wmofile.textures[ti].filename);

                        try
                        {
                            blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file), materials[i].filename + ".png"));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                        textureID++;
                    }
                }
            }

            //No idea how MTL files really work yet. Needs more investigation.
            foreach (var material in materials)
            {
                mtlsb.Append("newmtl " + material.filename + "\n");
                mtlsb.Append("Ns 96.078431\n");
                mtlsb.Append("Ka 1.000000 1.000000 1.000000\n");
                mtlsb.Append("Kd 0.640000 0.640000 0.640000\n");
                mtlsb.Append("Ks 0.000000 0.000000 0.000000\n");
                mtlsb.Append("Ke 0.000000 0.000000 0.000000\n");
                mtlsb.Append("Ni 1.000000\n");
                mtlsb.Append("d 1.000000\n");
                mtlsb.Append("illum 2\n");
                mtlsb.Append("map_Kd " + material.filename + ".png\n");
                if (material.transparent)
                {
                    mtlsb.Append("map_d " + material.filename + ".png\n");
                }
            }

            File.WriteAllText(Path.Combine(outdir, file.Replace(".wmo", ".mtl")), mtlsb.ToString());

            exportworker.ReportProgress(75, "Exporting model..");

            int numRenderbatches = 0;
            //Get total amount of render batches
            for (int i = 0; i < reader.wmofile.group.Count(); i++)
            {
                if (reader.wmofile.group[i].mogp.renderBatches == null) { continue; }
                numRenderbatches = numRenderbatches + reader.wmofile.group[i].mogp.renderBatches.Count();
            }


            int rb = 0;
            for (int g = 0; g < reader.wmofile.group.Count(); g++)
            {
                groups[g].renderBatches = new RenderBatch[numRenderbatches];

                var group = reader.wmofile.group[g];
                if (group.mogp.renderBatches == null) { continue; }
                for (int i = 0; i < group.mogp.renderBatches.Count(); i++)
                {
                    var batch = group.mogp.renderBatches[i];

                    groups[g].renderBatches[rb].firstFace = batch.firstFace;
                    groups[g].renderBatches[rb].numFaces = batch.numFaces;

                    if (batch.flags == 2)
                    {
                        groups[g].renderBatches[rb].materialID = (uint)batch.possibleBox2_3;
                    }
                    else
                    {
                        groups[g].renderBatches[rb].materialID = batch.materialID;
                    }
                    groups[g].renderBatches[rb].blendType = reader.wmofile.materials[batch.materialID].blendMode;
                    groups[g].renderBatches[rb].groupID = (uint)g;
                    rb++;
                }
            }

            exportworker.ReportProgress(95, "Writing files..");

            var objsw = new StreamWriter(Path.Combine(outdir, file.Replace(".wmo", ".obj")));
            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file) + ".mtl");

            foreach (var group in groups)
            {
                if(group.vertices == null) { continue; }
                objsw.WriteLine("g " + group.name);

                foreach (var vertex in group.vertices)
                {
                    objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                    objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                    objsw.WriteLine("vn " + vertex.Normal.X + " " + vertex.Normal.Y + " " + vertex.Normal.Z);
                }

                var indices = group.indices;

                foreach (var renderbatch in group.renderBatches)
                {
                    var i = renderbatch.firstFace;
                    if (renderbatch.numFaces > 0)
                    {
                        objsw.WriteLine("usemtl " + materials[renderbatch.materialID].filename);
                        objsw.WriteLine("s 1");
                        while (i < (renderbatch.firstFace + renderbatch.numFaces))
                        {
                            objsw.WriteLine("f " + (indices[i] + group.verticeOffset + 1) + "/" + (indices[i] + group.verticeOffset + 1) + "/" + (indices[i] + group.verticeOffset + 1) + " " + (indices[i + 1] + group.verticeOffset + 1) + "/" + (indices[i + 1] + group.verticeOffset + 1) + "/" + (indices[i + 1] + group.verticeOffset + 1) + " " + (indices[i + 2] + group.verticeOffset + 1) + "/" + (indices[i + 2] + group.verticeOffset + 1) + "/" + (indices[i + 2] + group.verticeOffset + 1));
                            i = i + 3;
                        }
                    }
                }
            }
            objsw.Close();
            Console.WriteLine("Done loading WMO file!");
        }

        public void ADTtoOBJ(string file)
        {
            float TileSize = 1600.0f / 3.0f; //533.333
            float ChunkSize = TileSize / 16.0f; //33.333
            float UnitSize = ChunkSize / 8.0f; //4.166666 // ~~fun fact time with marlamin~~ this /2 ends up being pixelspercoord on minimap
            float MapMidPoint = 32.0f / ChunkSize;

            var mapname = file.Replace("world\\maps\\", "").Substring(0, file.Replace("world\\maps\\", "").IndexOf("\\"));
            var centerx = int.Parse(file.Substring(file.Length - 9, 2));
            var centery = int.Parse(file.Substring(file.Length - 6, 2));

            List<RenderBatch> renderBatches = new List<RenderBatch>();
            List<Vertex> verticelist = new List<Vertex>();
            List<int> indicelist = new List<Int32>();
            Dictionary<int, string> materials = new Dictionary<int, string>();

            var distance = 1;

            // Create output directory
            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            for (int y = centery; y < centery + distance; y++)
            {
                for (int x = centerx; x < centerx + distance; x++)
                {
                    var curfile = "world\\maps\\" + mapname + "\\" + mapname + "_" + x + "_" + y + ".adt";

                    if (!CASC.FileExists(file))
                    {
                        continue;
                    }

                    exportworker.ReportProgress(0, "Loading ADT " + curfile);

                    ADTReader reader = new ADTReader();
                    reader.LoadADT(curfile);

                    // No chunks? Let's get the hell out of here
                    if (reader.adtfile.chunks == null)
                    {
                        continue;
                    }
                    if (CASC.FileExists("world\\maptextures\\" + mapname + "\\" + mapname + "_" + y + "_" + x + ".blp"))
                    {
                        materials.Add(materials.Count() + 1, "mat" + y.ToString() + x.ToString());

                        var blpreader = new BLPReader();

                        blpreader.LoadBLP(curfile.Replace("maps", "maptextures").Replace(".adt", ".blp"));

                        try
                        {
                            blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file), "mat" + y.ToString() + x.ToString() + ".png"));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    else
                    {
                        //ADT exporting requires Legion! If Legion, map does not support new view distance tech so we can't export textures either.
                        continue;
                    }
                   
                    //List<Material> materials = new List<Material>();

                    //for (int ti = 0; ti < reader.adtfile.textures.filenames.Count(); ti++)
                    //{
                    //    Material material = new Material();
                    //    material.filename = reader.adtfile.textures.filenames[ti];

                    //    //if (!WoWFormatLib.Utils.CASC.FileExists(material.filename)) { continue; }

                    //    material.textureID = BLPLoader.LoadTexture(reader.adtfile.textures.filenames[ti], cache);

                    //    materials.Add(material);
                    //}

                    var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
                    var initialChunkX = reader.adtfile.chunks[0].header.position.X;

                    for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
                    {
                        var chunk = reader.adtfile.chunks[c];

                        int off = verticelist.Count();

                        RenderBatch batch = new RenderBatch();

                        for (int i = 0, idx = 0; i < 17; i++)
                        {
                            for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                            {
                                Vertex v = new Vertex();
                                v.Normal = new Vector3(chunk.normals.normal_0[idx], chunk.normals.normal_1[idx], chunk.normals.normal_2[idx]);
                                v.Position = new Vector3(chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z, chunk.header.position.X - (i * UnitSize * 0.5f));
                                if ((i % 2) != 0) v.Position.X -= 0.5f * UnitSize;
                                v.TexCoord = new Vector3(-(v.Position.X - initialChunkX) / TileSize, -(v.Position.Z - initialChunkY) / TileSize, 0.0f);
                                verticelist.Add(v);
                            }
                        }

                        batch.firstFace = (uint)indicelist.Count();

                        for (int j = 9; j < 145; j++)
                        {
                            indicelist.AddRange(new Int32[] { off + j + 8, off + j - 9, off + j });
                            indicelist.AddRange(new Int32[] { off + j - 9, off + j - 8, off + j });
                            indicelist.AddRange(new Int32[] { off + j - 8, off + j + 9, off + j });
                            indicelist.AddRange(new Int32[] { off + j + 9, off + j + 8, off + j });
                            if ((j + 1) % (9 + 8) == 0) j += 9;
                        }

                        batch.materialID = (uint) materials.Count();

                        batch.numFaces = (uint)(indicelist.Count()) - batch.firstFace;

                        //var layermats = new List<uint>();
                        //var alphalayermats = new List<int>();

                        //for (int li = 0; li < reader.adtfile.texChunks[c].layers.Count(); li++)
                        //{
                        //    if (reader.adtfile.texChunks[c].alphaLayer != null)
                        //    {
                        //        alphalayermats.Add(BLPLoader.GenerateAlphaTexture(reader.adtfile.texChunks[c].alphaLayer[li].layer));
                        //    }
                        //    layermats.Add((uint)cache.materials[reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[li].textureId].ToLower()]);
                        //}

                        //batch.materialID = layermats.ToArray();
                        //batch.alphaMaterialID = alphalayermats.ToArray();

                        renderBatches.Add(batch);
                    }
                }
            }

            var mtlsw = new StreamWriter(Path.Combine(outdir, file.Replace(".adt", ".mtl")));

            //No idea how MTL files really work yet. Needs more investigation.
            foreach(var material in materials)
            {
                mtlsw.WriteLine("newmtl " + material.Value);
                mtlsw.WriteLine("Ka 1.000000 1.000000 1.000000");
                mtlsw.WriteLine("Kd 0.640000 0.640000 0.640000");
                mtlsw.WriteLine("map_Ka " + material.Value + ".png");
                mtlsw.WriteLine("map_Kd " + material.Value + ".png");
            }

            mtlsw.Close();

            var indices = indicelist.ToArray();

            var adtname = Path.GetFileNameWithoutExtension(file);

            var objsw = new StreamWriter(Path.Combine(outdir, file.Replace(".adt", ".obj")));

            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file) + ".mtl");
            objsw.WriteLine("g " + adtname);

            foreach (var vertex in verticelist)
            {
                objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                objsw.WriteLine("vn " + vertex.Normal.X + " " + vertex.Normal.Y + " " + vertex.Normal.Z);
            }

            foreach (var renderBatch in renderBatches)
            {
                var i = renderBatch.firstFace;
                if (materials.ContainsKey((int)renderBatch.materialID)) { objsw.WriteLine("usemtl " + materials[(int)renderBatch.materialID]); objsw.WriteLine("s 1"); }
                while (i < (renderBatch.firstFace + renderBatch.numFaces))
                {
                    objsw.WriteLine("f " + (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1) + " " + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1));
                    i = i + 3;
                }
            }

            objsw.Close();
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if(m2CheckBox == null) { return; }
            if ((bool) adtCheckBox.IsChecked) { showADT = true; } else { showADT = false; }
            if ((bool) m2CheckBox.IsChecked) { showM2 = true; } else { showM2 = false; }
            if ((bool) wmoCheckBox.IsChecked) { showWMO = true; } else { showWMO = false; }

            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Visibility = Visibility.Visible;
            exportButton.Visibility = Visibility.Hidden;
            modelListBox.Visibility = Visibility.Hidden;
            filterTextBox.Visibility = Visibility.Hidden;
            wmoCheckBox.Visibility = Visibility.Hidden;
            m2CheckBox.Visibility = Visibility.Hidden;
            adtCheckBox.Visibility = Visibility.Hidden;

            files = new List<String>();

            worker.RunWorkerAsync();
        }

        public struct RenderBatch
        {
            public uint firstFace;
            public uint materialID;
            public uint numFaces;
            public uint groupID;
            public uint blendType;
        }

        public struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 TexCoord;
            public Vector3 Color;
        }

        public struct Material
        {
            public string filename;
            public WoWFormatLib.Structs.M2.TextureFlags flags;
            public int textureID;
            public bool transparent;
        }

        public struct WMOGroup
        {
            public string name;
            public uint verticeOffset;
            public Vertex[] vertices;
            public uint[] indices;
            public RenderBatch[] renderBatches;
        }

        
    }
}
