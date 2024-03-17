using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aimidge.Pages
{
    public class GeneratorModel : PageModel
    {
        private readonly ILogger<GeneratorModel> _logger;

        public GeneratorModel(ILogger<GeneratorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
