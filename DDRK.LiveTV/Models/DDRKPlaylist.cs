using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class DDRKPlaylist
    {
        [JsonPropertyName("url")]
        public string URL { get; set; }

        [JsonPropertyName("pin")]
        public string Payload { get; set; }
    }
}
