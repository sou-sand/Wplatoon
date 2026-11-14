using System.Windows;
using System.Runtime.Serialization;
using System;

namespace Wplatoon
{
    [DataContract]
    public class ItemViewModel
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Subtitle { get; set; }
        [DataMember]
        public string ImagePath { get; set; }
        
        [DataMember]
        public string Stage1Name { get; set; }
        [DataMember]
        public string Stage2Name { get; set; }
        [DataMember]
        public string Image2Path { get; set; }
        [DataMember]
        public string TimeRange { get; set; }

        [DataMember]
        public string Extra { get; set; }
        public Visibility Stage2Visibility { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? RemindAt { get; set; }
    }
}