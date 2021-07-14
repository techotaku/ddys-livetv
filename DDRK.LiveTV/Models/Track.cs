using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class Track
    {
        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("video")]
        public string Video { get; set; }

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; }
    }
}
