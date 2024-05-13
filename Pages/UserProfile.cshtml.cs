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
        private readonly DatabaseService _databaseService;

        public ProfileModel(ILogger<ProfileModel> logger, DatabaseService dbService, CookieService cookieService, DatabaseService databaseService)
        {
            _logger = logger;
            _dbService = dbService;
            _cookieService = cookieService;
            _databaseService = databaseService;
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

        public async Task<IActionResult> OnGetGetInfoAsync()
        {
            string uid = _cookieService.ParseCookieUID("Cookie");
            if (!string.IsNullOrEmpty(uid))
            {
                Task<string> data = _databaseService.GetUserInfo(uid);
                string userInfo = await data;
                if (!string.IsNullOrEmpty(userInfo))
                {
                    return StatusCode(200, userInfo);
                }
                else
                {
                    return StatusCode(404);
                }
            }
            else
            {
                await _cookieService.SetCookie();
                uid = _cookieService.ParseCookieUID("Cookie");
                if (!string.IsNullOrEmpty(uid))
                {
                    return await OnGetGetInfoAsync();
                }
                else
                {
                    return StatusCode(404);
                }
            }
        }
    }
}
