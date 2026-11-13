using System;
using System.Linq;
using Microsoft.Phone.Shell;

namespace Wplatoon
{
    public static class TileHelper
    {
        public static void Update(ItemViewModel current)
        {
            if (current == null) return;
            try
            {
                var tile = ShellTile.ActiveTiles.FirstOrDefault();
                if (tile == null) return;

                tile.Update(new FlipTileData
                {
                    Title = "Wplatoon",
                    BackTitle = current.Title,
                    BackContent = current.Stage1Name + "\n" + current.Stage2Name,
                    WideBackContent = current.Stage1Name + ", " + current.Stage2Name + "\n" + current.Subtitle
                });
            }
            catch (Exception) { /* a tile failure should never crash the app */ }
        }
    }
}