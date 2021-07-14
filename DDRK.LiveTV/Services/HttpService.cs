using DDRK.LiveTV.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DDRK.LiveTV.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpService> _logger;

        public HttpService(HttpClient client, ILogger<HttpService> logger)
        {
            _httpClient = client;

            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
            _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.8");

            _httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\" Not;A Brand\";v=\"99\", \"Google Chrome\";v=\"91\", \"Chromium\";v=\"91\"");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");

            _logger = logger;
        }

        public async Task<Title> FetchTitle(string url)
        {
            _logger.LogInformation("\"{target}\": Trying to fetch title information...", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var caption = doc.DocumentNode.SelectSingleNode("//title").InnerText.Split('-')[0].Trim();
            var jsonNode = doc.DocumentNode.SelectSingleNode("//script[@class='wp-playlist-script' and @type='application/json']");
            if (jsonNode == null)
            {
                _logger.LogWarning("\"{target}\": Title information not found.", url);
                return null;
            }

            var json = jsonNode.InnerHtml;
            var ddrkTitle = JsonSerializer.Deserialize<DDRKTitle>(json);

            var title = new Title
            {
                Caption = caption,
                Tracks = new List<Track>(ddrkTitle.Tracks.Select(t => new Track { Caption = t.Caption, Video = t.VideoSource, Subtitle = t.SubtitleSource }))
            };
            return title;
        }

        public async Task<string> FetchVideo(string video)
        {
            _logger.LogInformation("\"{target}\": Trying to fetch media information...", video);

            var raw = $"{{\"path\":\"{video}\",\"expire\":{(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 600000L)}}}";
            var videoId = AES.Encrypt(raw.UTF8Encode(), "zevS%th@*8YWUm%K", "5080305495198718").Base64Encode().UrlEncode();
            var response = await _httpClient.GetAsync($"{RandomVideoServer}:9543/video?id={videoId}&type=mix");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var ddrkPlaylist = JsonSerializer.Deserialize<DDRKPlaylist>(json);
            if (!string.IsNullOrEmpty(ddrkPlaylist.URL))
            {
                return ddrkPlaylist.URL;
            }
            else if (!string.IsNullOrEmpty(ddrkPlaylist.Payload))
            {
                return ddrkPlaylist.Payload.Select(c => (byte)c).ToArray().Inflate().UTF8Decode();
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<byte[]> FetchSubtitle(string subtitle)
        {
            _logger.LogInformation("\"{target}\": Trying to fetch subtitle from Aliyun OSS...", subtitle);

            var response = await _httpClient.GetAsync($"https://ddrk.oss-accelerate.aliyuncs.com{subtitle}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Array.Empty<byte>();
            }

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            var hex = BitConverter.ToString(data[0..16]).Replace("-", "");
            var key = HexStringToByteArray(hex);
            var ciphertext = data.Skip(16).ToArray();
            var plaintext = AES.Decrypt(ciphertext, key, key);
            return plaintext.UnGzip();
        }

        public async Task<byte[]> FetchSegment(string target)
        {
            var response = await _httpClient.GetAsync(target);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsByteArrayAsync();
            if (HasPngHeader(data))
            {
                _logger.LogInformation("\"{target}\": Received media segment with PNG header, will remove it.", target);
                return TrySkipPngHeader(data);
            }
            else
            {
                return data;
            }
        }

        private static bool HasPngHeader(byte[] data)
        {
            return data != null && data.Length > 3 && 137 == data[0] && 80 == data[1] && 78 == data[2] && 71 == data[3];
        }

        private static byte[] TrySkipPngHeader(byte[] data)
        {
            if (0x47 == data[0])
            {
                return data;
            }
            
            if (137 == data[0] && 80 == data[1] && 78 == data[2] && 71 == data[3] && 96 == data[118] && 130 == data[119])
            {
                return data.Skip(120).ToArray();
            }

            if (137 == data[0] && 80 == data[1] && 78 == data[2] && 71 == data[3] && 96 == data[6100] && 130 == data[6101])
            {
                return data.Skip(6102).ToArray();
            }
            
            if (137 == data[0] && 80 == data[1] && 78 == data[2] && 71 == data[3] && 96 == data[67] && 130 == data[68])
            {
                return data.Skip(69).ToArray();
            }
           
            if (137 == data[0] && 80 == data[1] && 78 == data[2] && 71 == data[3] && 96 == data[769] && 130 == data[770])
            {
                return data.Skip(771).ToArray();
            }
            
            if (137 == data[0] && 80 == data[1] && 78 == data[2] && 71 == data[3])
            {
                // 手动查询结尾标记 (0x60 0x82 0x47)
                int skip = 0;
                for (int i = 4; i < data.Length - 3; i++)
                {
                    if (data[i] == 0x60 && data[i + 1] == 0x82 && data[i + 2] == 0x47)
                    {
                        skip = i + 2;
                        break;
                    }
                }
                return data.Skip(skip).ToArray();
            }

            return data;
        }

        private static byte[] HexStringToByteArray(string strHex)
        {
            var r = new byte[strHex.Length / 2];
            for (int i = 0; i <= strHex.Length - 1; i += 2)
            {
                r[i / 2] = Convert.ToByte(Convert.ToInt32(strHex.Substring(i, 2), 16));
            }
            return r;
        }

        private static string RandomVideoServer
        {
            get
            {
                return new Random().NextDouble() >= 0.4d ? "https://v3.ddrk.me" : "https://v2.ddrk.me";
            }
        }
    }
}
