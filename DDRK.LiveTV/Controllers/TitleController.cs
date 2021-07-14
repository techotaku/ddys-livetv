using DDRK.LiveTV.Models;
using DDRK.LiveTV.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DDRK.LiveTV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TitleController : ControllerBase
    {
        private readonly ILogger<TitleController> _logger;
        private readonly HttpService _httpService;

        public TitleController(ILogger<TitleController> logger, HttpService httpService)
        {
            _logger = logger;
            _httpService = httpService;
        }

        [HttpGet("{key}")]
        public async Task<ContentResult> Get(string key)
        {
            return Content(await FetchTitle(key), "text/html", Encoding.UTF8);
        }

        [HttpGet("{key}/{num}")]
        public async Task<ContentResult> Get(string key, ushort num)
        {
            return Content(await FetchTitle($"{key}/{num}"), "text/html", Encoding.UTF8);
        }

        private async Task<string> FetchTitle(string url)
        {
            var title = await _httpService.FetchTitle($"https://ddrk.me/{url}/");
            var html = new StringBuilder();

            html.AppendLine("<!doctype html>");
            html.AppendLine("<html lang=\"zh-CN\">");
            html.AppendLine("<head>");
            html.AppendLine($"<title>{title.Caption}</title>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div>");
            html.AppendLine($"<h1>{title.Caption}</h1>");
            html.AppendLine("</div>");
            html.AppendLine("<div>");
            html.AppendLine("<script type=\"text/javascript\">");
            html.AppendLine(@"
function updateClipboard(name, value) {
  navigator.clipboard.writeText(value).then(function() {
    alert('已复制 ' + name + ' 到剪贴板。');
  }, function() {
    alert('复制 ' + name + ' 到剪贴板时出错。');
  });
}
");
            html.AppendLine("</script>");
            html.AppendLine("<ul>");
            foreach (var track in title.Tracks)
            {
                var t = new TrackInUri { Video = track.Video, Subtitle = track.Subtitle };
                var key = JsonSerializer.Serialize(t).UrlSafeBase64EncodeUtf8String();
                var videoUrl = $"{Request.Scheme}://{Request.Host}" + "/video/media/" + key + "/" + track.Video.UrlEncode();
                var subtitle = $"{Request.Scheme}://{Request.Host}" + "/video/subtitle/" + track.Subtitle.UrlSafeBase64EncodeUtf8String();

                html.AppendLine("<li>");
                html.AppendLine($"<b>{track.Caption}</b>&nbsp;");
                html.AppendLine($"<a href=\"{videoUrl}\" target=\"_blank\">媒体</a>&nbsp;");
                html.AppendLine($"<button onclick=\"updateClipboard('{(track.Caption + " 媒体地址")}','{videoUrl}')\">复制</button>&nbsp;|&nbsp;");
                html.AppendLine($"<a href=\"{subtitle}\" target=\"_blank\">字幕</a>&nbsp;");
                html.AppendLine($"<button onclick=\"updateClipboard('{(track.Caption + " 字幕地址")}','{subtitle}')\">复制</button>");
                html.AppendLine("</li>");
            }
            html.AppendLine("</ul>");
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}
