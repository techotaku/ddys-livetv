using DDRK.LiveTV.Models;
using DDRK.LiveTV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DDRK.LiveTV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TitleController : ControllerBase
    {
        private readonly ILogger<TitleController> _logger;
        private readonly HttpService _httpService;

        public TitleController(ILogger<TitleController> logger, HttpService httpService)
        {
            _logger = logger;
            _httpService = httpService;
        }

        [HttpPost("fetch")]
        public async Task<Title> Fetch([FromForm] string url)
        {
            return await _httpService.FetchTitle(url);
        }
    }
}
