using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class DDRKDimensionValue
    {
        [JsonPropertyName("width")]
        public string Width { get; set; }

        [JsonPropertyName("height")]
        public string Height { get; set; }
    }
}
