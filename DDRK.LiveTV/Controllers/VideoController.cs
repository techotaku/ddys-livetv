using DDRK.LiveTV.Models;
using DDRK.LiveTV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DDRK.LiveTV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly ILogger<VideoController> _logger;
        private readonly HttpService _httpService;

        public VideoController(ILogger<VideoController> logger, HttpService httpService)
        {
            _logger = logger;
            _httpService = httpService;
        }

        [HttpGet("media/{key}/{decoration}")]
        public async Task<IActionResult> Media(string key)
        {
            var t = JsonSerializer.Deserialize<TrackInUri>(key.UrlSafeBase64DecodeUtf8String());
            var video = await _httpService.FetchVideo(t.Video);
            if (video.StartsWith("https://"))
            {
                _logger.LogInformation("\"{target}\": Received media file address, will redirect to it.", t.Video);
                return Redirect(video);
            }
            else
            {
                _logger.LogInformation("\"{target}\": Received playlist, will regenerate it.", t.Video);
                var prefix = $"{Request.Scheme}://{Request.Host}";
                var playlist = HandlePlaylist(prefix, video, t.Subtitle);
                return File(playlist, "application/vnd.apple.mpegurl", $"{t.Video.Replace(".mp4", string.Empty)}.m3u8");
            }
        }

        private static byte[] HandlePlaylist(string prefix, string raw, string subtitle)
        {
            MediaPlaylist playlist = null;
            var lines = raw.Split('\r', '\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            for(int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (playlist == null && line.StartsWith("#EXT-X-TARGETDURATION:"))
                {
                    playlist = new MediaPlaylist(line.Replace("#EXT-X-TARGETDURATION:", string.Empty));
                }
                else if (playlist != null && line.StartsWith("#EXTINF:") && i < lines.Length - 1)
                {
                    var key = lines[i + 1].UrlSafeBase64EncodeUtf8String();
                    var url = prefix + "/video/segment/" + key;
                    playlist.AddMedia(line.Replace("#EXTINF:", string.Empty), url);
                }
            }
            return playlist.ToBytes();
        }

        [HttpGet("subtitle/{key}")]
        public async Task<IActionResult> Subtitle(string key)
        {
            var target = key.UrlSafeBase64DecodeUtf8String();
            var subtitle = await _httpService.FetchSubtitle(target);
            if (subtitle == null || subtitle.Length <= 0)
            {
                _logger.LogWarning("\"{target}\": Subtitle not found.", target);
                return NotFound();
            }
            else
            {
                return File(subtitle, "text/vtt;charset=utf-8", $"{target.Replace(".ddr", string.Empty)}.vtt");
            }
        }

        [HttpGet("segment/{key}")]
        public async Task<IActionResult> Segment(string key)
        {
            var target = key.UrlSafeBase64DecodeUtf8String();
            var data = await _httpService.FetchSegment(target);
            if (data == null || data.Length <= 0)
            {
                _logger.LogWarning("\"{target}\": Segment not found.", target);
                return NotFound();
            }
            else
            {
                return File(data, "video/mp4", $"{key}.ts");
            }
        }
    }
}
