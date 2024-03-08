using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageGPT;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json.Serialization;


namespace ImageGPT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        public class PromptModel
        {
            public string Prompt { get; set; }
        }

        [HttpPost("GetPrompt")]
        public async Task<IActionResult> GetPrompt([FromBody] PromptModel userPrompt)
        {
            try
            {
                string prompt = userPrompt?.Prompt;
                string apiUrl = "http://193.161.193.99:61464/sdapi/v1/txt2img";

                string jsonPayload = @"
                {
                    ""prompt"": """ + prompt + @""",
                    ""steps"": 15,
                    ""width"": 512,
                    ""height"": 512,
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
    }
}