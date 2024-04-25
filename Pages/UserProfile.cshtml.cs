using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Principal;

namespace Aimidge.Pages
{
    [IgnoreAntiforgeryToken]
    public class ProfileModel : PageModel
    {
        private readonly ILogger<ProfileModel> _logger;
        private readonly DatabaseService _dbService;
        private readonly CookieService _cookieService;

        public ProfileModel(ILogger<ProfileModel> logger, DatabaseService dbService, CookieService cookieService)
        {
            _logger = logger;
            _dbService = dbService;
            _cookieService = cookieService;
        }

        public class UserData
        {
            public string Username { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostUpdateDataAsync([FromBody] UserData data)
        {
            try
            {
                Console.WriteLine(data.Username);
                string cookie = _cookieService.ParseCookieUID("Cookie");
                bool status = await _dbService.UpdateUserData(cookie, data.Username, data.Name, data.Email);
                if (status)
                {
                    return StatusCode(200);
                }
                else
                {
                    return StatusCode(500);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occured in OnPostUpdateDataAsync method: " + ex.Message);
            }
        }
    }
}
