using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageGPT;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json.Serialization;
using Aimidge.Services;
using System.Text.RegularExpressions;


namespace ImageGPT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly CookieService _cookieService;

        public ApiController(ILogger<ApiController> logger, CookieService cookieService)
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

				string apiUrl = "http://193.161.193.99:61464/sdapi/v1/txt2img";

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

                using(var httpClient = new HttpClient()) 
                {
                    httpClient.DefaultRequestHeaders.Add("txt2img", "application/json");
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);
                    if(response.IsSuccessStatusCode)
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

        [HttpPost("SetUpdatedCookie")]
        public async Task<IActionResult> SetUpdatedCookie()
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

        [HttpGet("GetCookie")]
        public async Task<IActionResult> GetCookie()
        {
            try 
            {
                var cookie = await _cookieService.ParseCookie("Cookie");
                Console.WriteLine(cookie);
                if(!string.IsNullOrEmpty(cookie))
                {
                    return Ok(cookie);
                }
                return Ok();
            }
            catch(Exception ex) 
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}