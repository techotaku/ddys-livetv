using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class DDRKMeta
    {
        [JsonPropertyName("length_formatted")]
        public string LengthFormatted { get; set; }
    }
}
