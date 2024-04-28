using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System;
using System.Diagnostics;

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
            ViewData["Home"] = _localizer["Home"];
        }

        public async Task<IActionResult> OnGetGetGalleryAsync(string imgName)
        {
            try
            {
                string base64Image = await SDService.GetImg(_cookieService.ParseCookieUID("Cookie"), imgName);
                if (!string.IsNullOrEmpty(base64Image))
                {
                    return new JsonResult(new { image = base64Image });

                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get gallery error: {ex.Message}");
                return NotFound($"{ex.Message}");
            }
        }

        public async Task<IActionResult> OnGetGetGalleryNamesAsync()
        {
            try
            {
                List<string> imgNames = await SDService.GetImgNames(_cookieService.ParseCookieUID("Cookie"));
                return new JsonResult(imgNames);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get gallery names error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
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
