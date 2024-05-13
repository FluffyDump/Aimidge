using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using static Aimidge.Controllers.AccountController;

namespace Aimidge.Pages
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly DatabaseService _databaseService;
        private readonly CookieService _cookieService;


        public LoginModel(ILogger<LoginModel> logger, DatabaseService databaseService, CookieService cookieService)
        {
            _logger = logger;
            _databaseService = databaseService;
            _cookieService = cookieService;
        }

        public void OnGet()
        {
        }

        public class UserRegistrationModel
        {
            public string Name { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string PasswordHash { get; set; }
        }

        public class UserLogInModel
        {
            public string Email { get; set; }
            public string PasswordHash { get; set; }
        }

        public async Task<IActionResult> OnPostAddAsync([FromBody] UserRegistrationModel model)
        {
            try
            {
                string password = CryptoService.Encrypt(model.PasswordHash);
                string value = await _databaseService.AddNewUser(model.Name, model.Username, model.Email, password);
                if (!value.Equals("UserExists"))
                {
                    _cookieService.RemoveCookie("Cookie");
                    await _cookieService.SetRegisteredCookie(value);
                    return StatusCode(200);
                }
                else if (value.Equals("UserExists"))
                {
                    return StatusCode(409);
                }
                else
                {
                    _logger.LogError("Failed to add new user in Login/Add");
                    return BadRequest("Failed to add user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in RegistrationController Add method, error: {ex.Message}");
                return BadRequest();
            }
        }

        public async Task<IActionResult> OnPostAuthAsync([FromBody] UserLogInModel model)
        {
            try
            {
                string password = CryptoService.Encrypt(model.PasswordHash);
                string value = await _databaseService.AuthenticateUser(model.Email, password);
                Console.WriteLine(value);
                if (!value.Equals("NotFound"))
                {
                    _cookieService.RemoveCookie("Cookie");
                    await _cookieService.SetRegisteredCookie(value);
                    return StatusCode(200);
                }
                else if (value.Equals("NotFound"))
                {
                    return StatusCode(404);
                }
                else
                {
                    _logger.LogError("Failed to add authenticate user in Login/Auth");
                    return BadRequest("Failed to authenticate user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in RegistrationController Auth method, error: {ex.Message}");
                return BadRequest();
            }
        }
    }
}
