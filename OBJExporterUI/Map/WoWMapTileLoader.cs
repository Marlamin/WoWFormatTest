using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MapControl;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace OBJExporterUI
{
    class WoWMapTileLoader : ITileImageLoader
    {
        private readonly ConcurrentStack<Tile> pendingTiles = new ConcurrentStack<Tile>();
        private int taskCount;

        public void LoadTilesAsync(MapTileLayer tileLayer)
        {
            pendingTiles.Clear();

            var tileSource = tileLayer.TileSource;
            var sourceName = tileLayer.SourceName;
            var tiles = tileLayer.Tiles.Where(t => t.Pending);

            if (tileSource != null && tiles.Any())
            {
                pendingTiles.PushRange(tiles.Reverse().ToArray());

                Func<Tile, Task> loadFunc;

                //if (Cache != null && !string.IsNullOrEmpty(sourceName) &&
                //    tileSource.UriFormat != null && tileSource.UriFormat.StartsWith("http"))
                //{
                //    loadFunc = tile => LoadCachedTileImageAsync(tile, tileSource, sourceName);
                //}
                //else
                //{
                    loadFunc = tile => LoadTileImageAsync(tile, tileSource);
                //}

                var newTasks = Math.Min(pendingTiles.Count, 4) - taskCount;

                while (--newTasks >= 0)
                {
                    Interlocked.Increment(ref taskCount);

                    var task = Task.Run(() => LoadTilesAsync(loadFunc)); // do not await
                }

                //Debug.WriteLine("{0}: {1} tasks", Environment.CurrentManagedThreadId, taskCount);
            }
        }

        private async Task LoadTilesAsync(Func<Tile, Task> loadTileImageFunc)
        {
            Tile tile;

            while (pendingTiles.TryPop(out tile))
            {
                tile.Pending = false;

                try
                {
                    await loadTileImageFunc(tile);
                    Console.WriteLine("TileImageLoader: {0}/{1}/{2}", tile.ZoomLevel, tile.XIndex, tile.Y);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("TileImageLoader: {0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }

            Interlocked.Decrement(ref taskCount);
        }

        private async Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            SetTileImage(tile, await LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel));
        }

        private void SetTileImage(Tile tile, ImageSource imageSource)
        {
            tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(imageSource));
        }

        public virtual async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {

            return await LoadImageAsync(@"world\minimaps\" + "azeroth" + "\\map32_32.blp");
           
            
            //var uri = GetUri(x, y, zoomLevel);

            //if (uri != null)
            //{
            //    imageSource = await ImageLoader.LoadImageAsync(uri);
            //}

            //return imageSource;
        }

        public static async Task<ImageSource> LoadImageAsync(string path)
        {
            ImageSource imageSource = null;

            try
            {
                var blp = new BLPReader();
                if (CASC.FileExists(path))
                {
                    blp.LoadBLP(path);
                }
                else
                {
                    blp.LoadBLP(204207);
                }
                var bitmapImage = new BitmapImage();

                using (var bitmap = blp.asBitmapStream())
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = bitmap;
                    bitmapImage.DecodePixelHeight = 256;
                    bitmapImage.DecodePixelWidth = 256;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }

                imageSource = bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ImageLoader: {0}: {1}", path, ex.Message);
            }
            return imageSource;
        }
    }
}
