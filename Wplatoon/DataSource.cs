using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace Wplatoon
{
    public static class DataSource
    {
        public static ItemViewModel Selected { get; set; }

        public static List<ItemViewModel> LoadSchedules()
        {
            var asm = Assembly.GetExecutingAssembly();

            string name = null;
            foreach (var n in asm.GetManifestResourceNames())
            {
                if (n.EndsWith("temp.json", StringComparison.OrdinalIgnoreCase))
                {
                    name = n;
                    break;
                }
            }

            if (name == null)
                throw new Exception("temp.json not found. Resources: " +
                    string.Join(", ", asm.GetManifestResourceNames()));

            using (Stream stream = asm.GetManifestResourceStream(name))
            {
                var serializer = new DataContractJsonSerializer(typeof(List<ItemViewModel>));
                return (List<ItemViewModel>)serializer.ReadObject(stream);
            }
        }
    }
}