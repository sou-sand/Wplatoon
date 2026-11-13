using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Windows;

namespace Wplatoon
{
    public static class DataSource
    {
        public static ItemViewModel Selected { get; set; }

        public static List<ItemViewModel> LoadSchedules()
        {
            var info = Application.GetResourceStream(
                new Uri("/temp/temp.json", UriKind.Relative));

            using (var stream = info.Stream)
            {
                var serializer = new DataContractJsonSerializer(typeof(List<ItemViewModel>));
                return (List<ItemViewModel>)serializer.ReadObject(stream);
            }
        }
    }

}