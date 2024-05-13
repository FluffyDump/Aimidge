using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace Aimidge.Pages
{
    [IgnoreAntiforgeryToken]
    public class AboutInsideModel : PageModel
    {
        private readonly ILogger<AboutInsideModel> _logger;
        private readonly CookieService _cookieService;
        private readonly IStringLocalizer<GeneratorModel> _localizer;
        private readonly DatabaseService _databaseService;

        public AboutInsideModel(ILogger<AboutInsideModel> logger,
                                CookieService cookieService,
                                IStringLocalizer<GeneratorModel> localizer,
                                DatabaseService databaseService)
        {
            _logger = logger;
            _cookieService = cookieService;
            _localizer = localizer;
            _databaseService = databaseService;
        }

        public void OnGet()
        {
            ViewData["Home"] = _localizer["Home"];
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
    }
}
