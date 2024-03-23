using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Aimidge.Pages
{
    public class GeneratorModel : PageModel
    {
        private readonly ILogger<GeneratorModel> _logger;
        private readonly IStringLocalizer<GeneratorModel> _localizer;

        public GeneratorModel(
            ILogger<GeneratorModel> logger,
            IStringLocalizer<GeneratorModel> localizer
        )
        {
            _logger = logger;
            _localizer = localizer;
        }

        public void OnGet()
        {
            ViewData["Home"] = _localizer["Home"];
        }
    }
}
