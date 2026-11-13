using System.Runtime.Serialization;

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
    }
}