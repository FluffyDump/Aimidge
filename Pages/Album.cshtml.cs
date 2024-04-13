using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Aimidge.Pages
{
    public class AlbumModel : PageModel
    {
        private readonly ILogger<AlbumModel> _logger;
        private readonly IStringLocalizer<AlbumModel> _localizer;

        public AlbumModel(
            ILogger<AlbumModel> logger,
            IStringLocalizer<AlbumModel> localizer
        )
        {
            _logger = logger;
            _localizer = localizer;
        }

        public void OnGet()
        {
        }
    }
}
