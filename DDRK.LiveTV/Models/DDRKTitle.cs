using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class DDRKTitle
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("tracklist")]
        public bool Tracklist { get; set; }

        [JsonPropertyName("tracknumbers")]
        public bool Tracknumbers { get; set; }

        [JsonPropertyName("images")]
        public bool Images { get; set; }

        [JsonPropertyName("artists")]
        public bool Artists { get; set; }

        [JsonPropertyName("tracks")]
        public List<DDRKTrack> Tracks { get; set; }
    }
}
