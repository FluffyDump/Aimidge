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
        private readonly SDService _sdService;
        private readonly TranslationService _translationService;
        private readonly ValidationService _validationService;

        public GeneratorModel(
            ILogger<GeneratorModel> logger,
            IStringLocalizer<GeneratorModel> localizer,
            CookieService cookieService,
            SDService sdService,
            TranslationService translationService,
            ValidationService validationService

        )
        {
            _logger = logger;
            _localizer = localizer;
            _cookieService = cookieService;
            _sdService = sdService;
            _translationService = translationService;
            _validationService = validationService;
        }

        public void OnGet()
        {
            ViewData["Home"] = _localizer["Home"];
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
