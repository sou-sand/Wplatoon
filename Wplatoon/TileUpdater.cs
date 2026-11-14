using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using Microsoft.Phone.Shell;

namespace Wplatoon
{
    public static class TileUpdater
    {
        private const string Dir = "Shared/ShellContent";
        public static string LastError = "";

        // Downloads the stage image (once), then updates the tile. Always calls done().
        public static void Update(string rule, string s1, string s2, string line, string imageUrl, Action done)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) { Apply(rule, s1, s2, line, null); done(); return; }

                string name = "stage" + Math.Abs(imageUrl.GetHashCode()) + ".png";
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.DirectoryExists(Dir)) store.CreateDirectory(Dir);
                    if (store.FileExists(Dir + "/" + name))
                    {
                        Apply(rule, s1, s2, line, name);
                        done();
                        return;
                    }
                }

                var client = new WebClient();
                client.Headers["User-Agent"] = "Wplatoon/1.0 (Windows Phone 8 fan app; contact: YOUR-EMAIL-OR-SITE)";
                client.OpenReadCompleted += (s, e) =>
                {
                    try
                    {
                        string file = null;
                        if (e.Error == null)
                        {
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                foreach (var old in store.GetFileNames(Dir + "/stage*.png"))
                                    store.DeleteFile(Dir + "/" + old);   // keep only the current image

                                using (var output = store.OpenFile(Dir + "/" + name, FileMode.Create))
                                using (e.Result) e.Result.CopyTo(output);
                            }
                            file = name;
                        }
                        else LastError = "image download: " + e.Error.Message;

                        Apply(rule, s1, s2, line, file);
                    }
                    catch (Exception ex) { LastError = ex.Message; }
                    finally { done(); }
                };
                client.OpenReadAsync(new Uri(imageUrl));
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                done();
            }
        }

        private static void Apply(string rule, string s1, string s2, string line, string file)
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile == null) { LastError = "no tile found"; return; }

            var data = new FlipTileData
            {
                Title = "Wplatoon",
                BackTitle = rule,
                BackContent = s1 + "\n" + s2,
                WideBackContent = s1 + ", " + s2 + "\n" + line
            };

            if (file != null)
            {
                var uri = new Uri("isostore:/" + Dir + "/" + file, UriKind.Absolute);
                data.BackBackgroundImage = uri;
                data.WideBackBackgroundImage = uri;
            }

            try
            {
                tile.Update(data);
                LastError = "";
            }
            catch (Exception ex)
            {
                if (file == null) { LastError = ex.Message; return; }

                Apply(rule, s1, s2, line, null);   // image refused: keep the text-only tile
                if (LastError.Length == 0) LastError = "background image refused: " + ex.Message;
            }
        }
    }
}