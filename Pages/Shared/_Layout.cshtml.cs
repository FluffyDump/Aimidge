using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aimidge.Pages.Shared
{
    [IgnoreAntiforgeryToken]
    public class LayoutModel: PageModel
    {
        private readonly CookieService _cookieService;
        private readonly ILogger<LayoutModel> _logger;
        public LayoutModel(CookieService cookieService, ILogger<LayoutModel> logger)
        {
            _cookieService = cookieService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetGetCookieAsync()
        {
            try
            {
                await _cookieService.GetCookies();
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get progress error: {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}
