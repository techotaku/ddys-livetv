using DDRK.LiveTV.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DDRK.LiveTV.Services
{
    public class PlaylistService
    {
        private readonly HttpService _httpService;
        private readonly ILogger<PlaylistService> _logger;

        public PlaylistService(HttpService httpService, ILogger<PlaylistService> logger)
        {
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<PlaylistFile> GeneratePlaylist(string prefix, string videoSource)
        {
            var videoInfo = await _httpService.FetchVideo(videoSource);
            if (string.IsNullOrEmpty(videoInfo))
            {
                _logger.LogWarning("\"{target}\": Received nothing.", videoSource);
                return null;
            }

            if (videoInfo.StartsWith("https://"))
            {
                _logger.LogInformation("\"{target}\": Received media file address, will generate STRM playlist.", videoSource);
                return new PlaylistFile 
                { 
                    Content = MakeStrmPlaylist(videoInfo), 
                    FileExtension = ".strm" 
                };
            }
            else
            {
                _logger.LogInformation("\"{target}\": Received playlist, will regenerate it.", videoSource);
                return new PlaylistFile 
                { 
                    Content = RegeneratePlaylist(prefix + "/segment/", videoInfo), 
                    FileExtension = ".m3u8" 
                };
            }
        }

        private static byte[] MakeStrmPlaylist(string mediaUrl)
        {
            return mediaUrl.UTF8Encode();
        }

        private static byte[] RegeneratePlaylist(string prefix, string rawPlaylist)
        {
            MediaPlaylist playlist = null;
            var lines = rawPlaylist.Split('\r', '\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (playlist == null && line.StartsWith("#EXT-X-TARGETDURATION:"))
                {
                    playlist = new MediaPlaylist(line.Replace("#EXT-X-TARGETDURATION:", string.Empty));
                }
                else if (playlist != null && line.StartsWith("#EXTINF:") && i < lines.Length - 1)
                {
                    var key = lines[i + 1].UrlSafeBase64EncodeUtf8String();
                    var url = prefix + key;
                    playlist.AddMedia(line.Replace("#EXTINF:", string.Empty), url);
                }
            }
            return playlist.ToBytes();
        }
    }
}
