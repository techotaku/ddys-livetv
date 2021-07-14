using System.Collections.Generic;
using System.Text;

namespace DDRK.LiveTV.Models
{
    public abstract class Playlist
    {
        protected readonly List<string> _content = new();

        public override string ToString()
        {
            var m3u8 = new StringBuilder();
            m3u8.AppendLine("#EXTM3U");
            m3u8.AppendLine("#EXT-X-VERSION:3");
            foreach (var c in _content)
            {
                m3u8.AppendLine(c);
            }
            return m3u8.ToString();
        }

        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }
    }
}
