using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Aimidge;
using Aimidge.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

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

                string badword = ValidationService.CheckProfanity(prompt);
                if (!badword.Equals("ok"))
                {
                    _logger.LogInformation("Aptiktas netinkamas zodis: " + badword);
                    return BadRequest("Aptiktas netinkamas zodis");
                }

                string resolution = userPrompt?.Resolution;
                Match match = Regex.Match(resolution, @"(\d+)x(\d+)");
                string width = match.Groups[1].Value;
                string height = match.Groups[2].Value;

                Task<string> base64Image = SDService.PostToAPIAsync(prompt, width, height, _cookieService.ParseCookieUID("Cookie"));
                if (!base64Image.Equals("BadRequest"))
                {
                    return Ok(new { image = base64Image.Result });
                }
                _logger.LogInformation("Ok");
                return Ok();
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

                using (var httpClient = new HttpClient())
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

        [HttpPost("SaveImage")]
        public async Task<IActionResult> SaveImage()
        {
            try
            {
                var result = await SDService.SaveImg(_cookieService.ParseCookieUID("Cookie"));
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Save image error: {ex.Message}");
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
