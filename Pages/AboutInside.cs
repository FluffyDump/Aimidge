using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aimidge.Pages
{
    public class AboutInsideModel : PageModel
    {
        private readonly ILogger<AboutInsideModel> _logger;

        public AboutInsideModel(ILogger<AboutInsideModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }

}
