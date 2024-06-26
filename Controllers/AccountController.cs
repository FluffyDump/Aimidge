using Microsoft.AspNetCore.Mvc;
using Aimidge.Services;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Diagnostics;

namespace Aimidge.Controllers
{
    [ApiController]
    [Route("/api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly DatabaseService _databaseService;
        private readonly CookieService _cookieService;

        public AccountController(ILogger<AccountController> logger, DatabaseService databaseService, CookieService cookieService)
        {
            _logger = logger;
            _databaseService = databaseService;
            _cookieService = cookieService;
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

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] UserRegistrationModel model)
        {
            string password = CryptoService.Encrypt(model.PasswordHash);

            try
            {
                string value = await _databaseService.AddNewUser(model?.Name, model?.Username, model?.Email, password);
                if (!value.Equals("UserExists"))
                {
                    _cookieService.RemoveCookie("Cookie");
                    await _cookieService.SetRegisteredCookie(value);
                    return Ok("ok");
                }
                else if (value.Equals("UserExists"))
                {
                    return Ok(value);
                }
                else
                {
                    _logger.LogError("Failed to add new user in AccountController/Add");
                    return BadRequest("Failed to add user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in RegistrationController Add method, error: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpPost("Auth")]
        public async Task<IActionResult> Auth([FromBody] UserLogInModel model)
        {

            string password = CryptoService.Encrypt(model.PasswordHash);

            try
            {
                string value = await _databaseService.AuthenticateUser(model?.Email, password);
                if (!value.Equals("BadPassword") && !value.Equals("NotFound"))
                {
                    _cookieService.RemoveCookie("Cookie");
                    await _cookieService.SetRegisteredCookie(value);
                    return Ok("ok");
                }
                else if (value.Equals("BadPassword") || value.Equals("NotFound"))
                {
                    return Ok("IncorrectValues");
                }
                else
                {
                    _logger.LogError("Failed to add authenticate user in AccountController/Auth");
                    return BadRequest("Failed to authenticate user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in RegistrationController Auth method, error: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            try
            {
                _cookieService.RemoveCookie("Cookie");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during logout: {ex.Message}");
                return StatusCode(500);
            }
        }

        [HttpGet("GetInfo")]
        public async Task<IActionResult> GetInfo()
        {
            string uid = _cookieService.ParseCookieUID("Cookie");
            if (!String.IsNullOrEmpty(uid))
            {
                Task<string> data = _databaseService.GetUserInfo(uid);
                string userInfo = await data;
                char name = userInfo.ElementAt(0);

                if (!String.IsNullOrEmpty(userInfo))
                {
                    return Ok(userInfo);
                }
                else
                {
                    return Ok("User info not found");
                }
            }
            else
            {
                return Ok();
            }
        }
    }
}