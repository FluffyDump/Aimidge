using Aimidge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using static Aimidge.Controllers.AccountController;

namespace Aimidge.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly DatabaseService _databaseService;
        private readonly CookieService _cookieService;


        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
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
            Debug.WriteLine("Authenticates");
            string password = CryptoService.Encrypt(model.PasswordHash);

            try
            {
                string value = await _databaseService.AddNewUser(model?.Name, model?.Username, model?.Email, password);
                if (!value.Equals("UserExists"))
                {
                    _cookieService.RemoveCookie("Cookie");
                    await _cookieService.SetRegisteredCookie(value);
                    return new JsonResult("ok");
                }
                else if (value.Equals("UserExists"))
                {
                    return new JsonResult(value);
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


        public async Task<IActionResult> OnPostAuthAsyn([FromBody] UserLogInModel model)
        {
            Debug.WriteLine("Authenticates");
            string password = CryptoService.Encrypt(model.PasswordHash);

            try
            {
                string value = await _databaseService.AuthenticateUser(model?.Email, password);
                if (!value.Equals("BadPassword") && !value.Equals("NotFound"))
                {
                    _cookieService.RemoveCookie("Cookie");
                    await _cookieService.SetRegisteredCookie(value);
                    return new JsonResult("ok");
                }
                else if (value.Equals("BadPassword") || value.Equals("NotFound"))
                {
                    return new JsonResult("IncorrectValues");
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
