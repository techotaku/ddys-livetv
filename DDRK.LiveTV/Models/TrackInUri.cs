using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class TrackInUri
    {
        [JsonPropertyName("v")]
        public string Video { get; set; }

        [JsonPropertyName("s")]
        public string Subtitle { get; set; }
    }
}
