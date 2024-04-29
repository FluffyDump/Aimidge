using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace Aimidge.Pages
{
    [IgnoreAntiforgeryToken]
    public class GeneratorModel : PageModel
    {
        private readonly ILogger<GeneratorModel> _logger;
        private readonly IStringLocalizer<GeneratorModel> _localizer;
        private readonly CookieService _cookieService;
        private readonly TranslationService _translationService;
        private readonly ValidationService _validationService;
        private readonly DatabaseService _databaseService;


        public GeneratorModel(
            ILogger<GeneratorModel> logger,
            IStringLocalizer<GeneratorModel> localizer,
            CookieService cookieService,
            TranslationService translationService,
            ValidationService validationService,
            DatabaseService databaseService

        )
        {
            _logger = logger;
            _localizer = localizer;
            _cookieService = cookieService;
            _translationService = translationService;
            _validationService = validationService;
            _databaseService = databaseService;
        }

        public void OnGet()
        {
            ViewData["Home"] = _localizer["Home"];
        }

        public async Task<IActionResult> OnGetGetInfoAsync()
        {
            string uid = _cookieService.ParseCookieUID("Cookie");
            if (!String.IsNullOrEmpty(uid))
            {
                Task<string> data = _databaseService.GetUserInfo(uid);
                string userInfo = await data;
                char name = userInfo.ElementAt(0);

                if (!String.IsNullOrEmpty(userInfo))
                {
                    return new JsonResult(userInfo);
                }
                else
                {
                    return new JsonResult("User info not found");
                }
            }
            else
            {
                return StatusCode(403);
            }
        }

        public IActionResult OnPostLogout()
        {
            try
            {
                _cookieService.RemoveCookie("Cookie");
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during logout: {ex.Message}");
                return StatusCode(500);
            }
        }

        public class PromptModel
		{
			public string Prompt { get; set; }
			public string Resolution { get; set; }
		}
		public async Task<IActionResult> OnPostGetPromptAsync([FromBody] PromptModel userPrompt)
		{
			try
			{
                string prompt = userPrompt?.Prompt;

				string enPrompt = await _translationService.TranslatePrompt(prompt);

				if (enPrompt != String.Empty)
				{
					prompt = enPrompt;
				}

                bool correctPrompt = await _validationService.CheckPrompt(prompt);  //Ckecks if prompt is in english?

                if (!correctPrompt) 
				{
					return StatusCode(422); //it is not in english
				}

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

				Task<string> data = SDService.PostToAPIAsync(prompt, width, height, _cookieService.ParseCookieUID("Cookie"));

				string img = await data;

                if (img != String.Empty && img != "403")
				{
                    return new JsonResult(new { image = img });
                }
				else if (img == "403")
				{
					return StatusCode(403);
				}
				return BadRequest();
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex.ToString());
				return BadRequest(ex.Message);
			}
		}

        public async Task<IActionResult> OnGetGetProgressAsync()
        {
            try
            {
                string apiUrl = "http://193.161.193.99:61464/sd_progress";

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("progress", "application/json");
                    var response = await httpClient.GetAsync(apiUrl);

                    return new ContentResult
                    {
                        Content = await response.Content.ReadAsStringAsync(),
                        ContentType = "text/plain",
                        StatusCode = 200
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get progress error: {ex.Message}");
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostSaveImageAsync()
        {
            try
            {
                var response = await SDService.SaveImg(_cookieService.ParseCookieUID("Cookie"));
                if (response)
                {
                    return StatusCode(200);
                }
                else
                {
                    var setCookieResponse = await _cookieService.GetCookies();
                    if (response)
                    {
                        return StatusCode(200);
                    } else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occured in OnPostSaveImageAsync method: \n" + ex.Message);
            }
        }
    }
}
