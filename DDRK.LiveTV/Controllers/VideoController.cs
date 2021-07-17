using DDRK.LiveTV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DDRK.LiveTV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private static readonly char[] invalidFileNameChars = Path.GetInvalidFileNameChars().Where(c => c != '/').ToArray();

        private static string NormalizeName(string name)
        {
            return Path.Combine(name.Split('/')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => new string(s.Select(c => invalidFileNameChars.Contains(c) ? '_' : c).ToArray()))
                .ToArray());
        }

        private readonly ILogger<VideoController> _logger;
        private readonly HttpService _httpService;
        private readonly PlaylistService _playlistService;
        private readonly string _path;

        public VideoController(ILogger<VideoController> logger, HttpService httpService, PlaylistService playlistService, IConfiguration configuration)
        {
            _logger = logger;
            _httpService = httpService;
            _playlistService = playlistService;

            var path = configuration["Playlist:Path"];
            if (!string.IsNullOrEmpty(path))
            {
                _path = path;
            }
            else
            {
                _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data");
            }
        }

        private static string HandlePath(string path, string rawFilename, string extension)
        {
            var filename = NormalizeName(rawFilename);
            var rawExt = Path.GetExtension(filename);
            if (!string.IsNullOrEmpty(rawExt) && rawExt.Length > 0 && filename.Length > rawExt.Length)
            {
                filename = filename.Substring(0, filename.Length - rawExt.Length) + extension;
            }
            else
            {
                filename += extension;
            }
            filename = Path.Combine(path, filename);
            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }            
            return filename;
        }

        [HttpPost("media")]
        public async Task<string> Media([FromForm] string target)
        {
            var prefix = $"{Request.Scheme}://{Request.Host}";
            var v = target.UrlDecode();
            var playlist = await _playlistService.GeneratePlaylist(prefix, v);

            if (playlist != null)
            {
                var filename = HandlePath(_path, v, playlist.FileExtension);
                _logger.LogInformation("\"{target}\": Playlist save path: \"{filename}\".", target, filename);
                await System.IO.File.WriteAllBytesAsync(filename, playlist.Content);
                return filename.Substring(_path.Length + 1);
            }
            else
            {
                _logger.LogWarning("\"{target}\": Media not found.", target);
                return string.Empty;
            }
        }

        [HttpPost("subtitle")]
        public async Task<string> Subtitle([FromForm] string target)
        {
            var subtitle = await _httpService.FetchSubtitle(target.UrlDecode());
            if (subtitle == null || subtitle.Length <= 0)
            {
                _logger.LogWarning("\"{target}\": Subtitle not found.", target);
                return string.Empty;
            }
            else
            {
                var filename = HandlePath(_path, target, ".vtt");
                _logger.LogInformation("\"{target}\": Subtitle save path: \"{filename}\".", target, filename);
                await System.IO.File.WriteAllBytesAsync(filename, subtitle);
                return filename.Substring(_path.Length + 1);
            }
        }
    }
}
