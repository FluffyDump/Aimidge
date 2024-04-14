using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System;

namespace Aimidge.Pages
{
    public class AlbumModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AlbumModel> _logger;
        private readonly IStringLocalizer<AlbumModel> _localizer;
        private readonly CookieService _cookieService;
        public List<string> Images { get; set; } = new List<string>();

        public AlbumModel(ILogger<AlbumModel> logger, IStringLocalizer<AlbumModel> localizer, IHttpClientFactory httpClientFactory, CookieService cookieService)
        {
            _logger = logger;
            _localizer = localizer;
            _httpClient = httpClientFactory.CreateClient();
            _cookieService = cookieService;
        }

        public void OnGet()
        {

        }
    }
}
