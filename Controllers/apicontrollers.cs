using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageGPT;
using System.Text;
using Newtonsoft.Json;
using System.IO;


namespace ImageGPT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

                string jsonPayload = @"
                {
                    ""prompt"": """ + prompt + @""",
                    ""steps"": 30,
                    ""width"": 512,
                    ""height"": 512,
                    ""restore_faces"": true,
                    ""save_images"": false
                }";

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return BadRequest(ex.Message);
            }
        }
    }
}