using System.Text.Json.Serialization;

namespace DDRK.LiveTV.Models
{
    public class DDRKTrack
    {
        [JsonPropertyName("src")]
        public string Src { get; set; }

        [JsonPropertyName("src0")]
        public string VideoSource { get; set; }

        [JsonPropertyName("src1")]
        public string Src1 { get; set; }

        [JsonPropertyName("src2")]
        public string VideoKey { get; set; }

        [JsonPropertyName("src3")]
        public string Src3 { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("image")]
        public DDRKImage Image { get; set; }

        [JsonPropertyName("thumb")]
        public DDRKThumb Thumb { get; set; }

        [JsonPropertyName("meta")]
        public DDRKMeta Meta { get; set; }

        [JsonPropertyName("portn")]
        public string Portn { get; set; }

        [JsonPropertyName("srctype")]
        public string Srctype { get; set; }

        [JsonPropertyName("cut")]
        public string Cut { get; set; }

        [JsonPropertyName("vttshift")]
        public string Vttshift { get; set; }

        [JsonPropertyName("userIP")]
        public object UserIP { get; set; }

        [JsonPropertyName("subsrc")]
        public string SubtitleSource { get; set; }

        [JsonPropertyName("dimensions")]
        public DDRKDimension Dimensions { get; set; }
    }
}
