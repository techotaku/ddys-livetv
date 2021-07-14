using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class DDRKImage
    {
        [JsonPropertyName("src")]
        public string Src { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
