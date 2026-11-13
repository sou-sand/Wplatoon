using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace Wplatoon
{
    public static class CacheHelper
    {
        public static readonly TimeSpan MaxAge = TimeSpan.FromHours(1);

        public static void Save(string name, string content)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            using (var stream = store.OpenFile(name, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
            }
            IsolatedStorageSettings.ApplicationSettings[name + ".time"] = DateTime.Now;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
        
        public static string Load(string name)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(name)) return null;
                    using (var stream = store.OpenFile(name, FileMode.Open))
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (IsolatedStorageException)
            {
                return null;
            }
        }

        public static DateTime? GetTime(string name)
        {
            DateTime t;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(name + ".time", out t))
                return t;
            return null;
        }

        public static bool IsFresh(string name)
        {
            var t = GetTime(name);
            return t.HasValue && (DateTime.Now - t.Value) < MaxAge;
        }
    }
}