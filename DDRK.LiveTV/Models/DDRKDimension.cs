using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class DDRKDimension
    {
        [JsonPropertyName("original")]
        public DDRKDimensionValue Original { get; set; }

        [JsonPropertyName("resized")]
        public DDRKDimensionValue Resized { get; set; }
    }
}
