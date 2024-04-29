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

        public async Task<IActionResult> OnGetGetInfoAsync()
        {
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
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
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
            Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABABABABABABABA");
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
