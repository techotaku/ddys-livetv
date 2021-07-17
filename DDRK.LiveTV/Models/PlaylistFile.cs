namespace DDRK.LiveTV.Models
{
    public class PlaylistFile
    {
        /// <summary>
        /// File extension name with the dot '.'.
        /// </summary>
        public string FileExtension { get; set; }

        public byte[] Content { get; set; }
    }
}
