namespace Wplatoon
{
    public static class TileHelper
    {
        public static string LastError { get { return TileUpdater.LastError; } }

        public static void Update(ItemViewModel current)
        {
            if (current == null) return;
            TileUpdater.Update(current.Title, current.Stage1Name, current.Stage2Name,
                               current.Subtitle, current.ImagePath, () => { });
        }
    }
}