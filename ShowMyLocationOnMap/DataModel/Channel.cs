
namespace ShowMyLocationOnMap.DataModel
{
    using System;
    using Newtonsoft.Json;

    public class Channel
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }
    }
}

