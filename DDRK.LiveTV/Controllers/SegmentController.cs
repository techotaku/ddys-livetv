using DDRK.LiveTV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DDRK.LiveTV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SegmentController : ControllerBase
    {
        private readonly ILogger<SegmentController> _logger;
        private readonly HttpService _httpService;

        public SegmentController(ILogger<SegmentController> logger, HttpService httpService)
        {
            _logger = logger;
            _httpService = httpService;
        }

        [HttpGet("{key}")]
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
