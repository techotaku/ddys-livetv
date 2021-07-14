using System;

namespace DDRK.LiveTV.Models
{
    public class MediaPlaylist : Playlist
    {
        public MediaPlaylist(string targetDuration)
        {
            _content.Add("#EXT-X-MEDIA-SEQUENCE:0");
            _content.Add("#EXT-X-ALLOW-CACHE:YES");
            if (!string.IsNullOrEmpty(targetDuration))
            {
                _content.Add($"#EXT-X-TARGETDURATION:{targetDuration}");
            }
        }

        public void AddMedia(string duration, string url)
        {
            _content.Add($"#EXTINF:{duration.Trim(' ', ',')},{Environment.NewLine}{url}");
        }

        public override string ToString()
        {
            var content = base.ToString();
            return content + Environment.NewLine + "#EXT-X-ENDLIST" + Environment.NewLine;
        }
    }
}
