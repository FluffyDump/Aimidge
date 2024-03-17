using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aimidge.Pages
{
    public class AboutOutsideModel : PageModel
    {
        private readonly ILogger<AboutOutsideModel> _logger;

        public AboutOutsideModel(ILogger<AboutOutsideModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }

}
