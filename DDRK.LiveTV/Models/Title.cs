using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class Title
    {
        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("tracks")]
        public List<Track> Tracks { get; set; }
    }
}
