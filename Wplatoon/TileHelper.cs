using System;
using System.Linq;
using Microsoft.Phone.Shell;

namespace Wplatoon
{
    public static class TileHelper
    {
        public static string LastError = "";

        public static void Update(ItemViewModel current)
        {
            if (current == null) return;
            LastError = "";

            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile == null) { LastError = "no tile found"; return; }

            try
            {
                tile.Update(new FlipTileData
                {
                    Title = "Wplatoon",
                    BackTitle = current.Title,
                    BackContent = current.Stage1Name + "\n" + current.Stage2Name
                });
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return;
            }
            
            try
            {
                tile.Update(new FlipTileData
                {
                    WideBackContent = current.Stage1Name + ", " + current.Stage2Name + "\n" + current.Subtitle
                });
            }
            catch (Exception) { }
        }
    }
}