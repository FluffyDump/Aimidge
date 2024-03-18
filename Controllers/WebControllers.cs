using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Aimidge;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json.Serialization;
using Aimidge.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting.Server;
using System.Linq;

namespace Aimidge.Controllers
{
    [ApiController]
    [Route("api/web")]
    public class WebControllers : ControllerBase
    {
        private readonly ILogger<WebControllers> _logger;
        private readonly CookieService _cookieService;

        public WebControllers(ILogger<WebControllers> logger, CookieService cookieService)
        {
            _logger = logger;
            _cookieService = cookieService;
        }

        public class PromptModel
        {
            public string Prompt { get; set; }
            public string Resolution { get; set; }
        }

        [HttpPost("GetPrompt")]
        public async Task<IActionResult> GetPrompt([FromBody] PromptModel userPrompt)
        {
            try
            {
                string prompt = userPrompt?.Prompt;
                string resolution = userPrompt?.Resolution;

                string[] prompt_parts = prompt.Split(' ');
                string[] badWords = System.IO.File.ReadAllLines("Profanity.txt");
                foreach (string word in prompt_parts)
                {
                    if (badWords.Contains(word.ToLower()))
                    {
                        _logger.LogInformation("Aptiktas netinkamas zodis: " + word);
                        return BadRequest("Aptiktas netinkamas zodis");
                    }
                }

                string apiUrl = "http://193.161.193.99:61464/stable_diffusion";

                Match match = Regex.Match(resolution, @"(\d+)x(\d+)");

                string width = match.Groups[1].Value;

                string height = match.Groups[2].Value;

                string jsonPayload = @"
                {
                    ""prompt"": """ + prompt + @""",
                    ""steps"": 15,
                    ""width"": """ + width + @""",
                    ""height"": """ + height + @""",
                    ""restore_faces"": true,
                    ""save_images"": false
                }";

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("txt2img", "application/json");
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseData);
                        string base64Image = jsonResponse.images[0];
                        return Ok(new { image = base64Image });
                    }
                    _logger.LogInformation("Ok");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetProgress")]
        public async Task<IActionResult> GetProgress()
        {
            try
            {
                string apiUrl = "http://193.161.193.99:61464/sd_progress";

                using(var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("progress", "application/json");
                    var response = await httpClient.GetAsync(apiUrl);
                    return Ok(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get progress error: {ex.Message}");
                return NotFound();
            }
        }

        [HttpPost("SetCookie")]
        public async Task<IActionResult> SetCookie()
        {
            try
            {
                await _cookieService.SetCookie();
                return Ok("Cookie set succesfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("GetCookie")]
        public async Task<IActionResult> GetCookie()
        {
            try
            {
                var cookie = await _cookieService.ParseCookie("Cookie");
                if (!string.IsNullOrEmpty(cookie))
                {
                    await _cookieService.UpdateCookie(cookie);
                    return Ok("ok");
                }
                return Ok("404");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}