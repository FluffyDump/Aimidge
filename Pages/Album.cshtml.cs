using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System;

namespace Aimidge.Pages
{
    [IgnoreAntiforgeryToken]
    public class AlbumModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AlbumModel> _logger;
        private readonly IStringLocalizer<AlbumModel> _localizer;
        private readonly CookieService _cookieService;
        private readonly SDService _sdService;

        public AlbumModel(ILogger<AlbumModel> logger, IStringLocalizer<AlbumModel> localizer, IHttpClientFactory httpClientFactory, CookieService cookieService, SDService sdService)
        {
            _logger = logger;
            _localizer = localizer;
            _httpClient = httpClientFactory.CreateClient();
            _cookieService = cookieService;
            _sdService = sdService;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostRemoveImgAsync([FromBody] string imgName)
        {
            try
            {
                bool status = await _sdService.RemoveImg(_cookieService.ParseCookieUID("Cookie"), imgName);
                if(status)
                {
                    return StatusCode(200);
                }
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in OnPostRemoveImgAsync, error: {ex}");
                return StatusCode(500);
            }
        }
    }
}
