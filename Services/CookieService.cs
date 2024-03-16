using Microsoft.VisualBasic;
using Aimidge.Services;

namespace Aimidge.Services
{
	public class CookieService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly DatabaseService _databaseService;

		public CookieService(IHttpContextAccessor httpContextAccessor, DatabaseService databaseService)
		{
			_httpContextAccessor = httpContextAccessor;
			_databaseService = databaseService;
		}

		public async Task SetCookie()
		{
			string key = "Cookie";
			string temp = Guid.NewGuid().ToString();
			string value = CryptoService.Encrypt(temp);

			var option = new CookieOptions();

			option.Expires = DateTime.Now.AddMinutes(20);
			option.Secure = true;
			option.HttpOnly = true;
			option.SameSite = SameSiteMode.Strict;

			try
			{
				await Task.Run(() => _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, option));
				await _databaseService.AddUnregisteredUser(value);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error occured while adding cookie or adding user: " + ex.Message);
			}
		}

		public async Task UpdateCookie(string cookie)
		{
			string key = "Cookie";

			if(cookie != null)
			{
				var option = new CookieOptions();
				option.Expires = DateTime.Now.AddMinutes(20);
				try
				{
					await Task.Run(() => _httpContextAccessor.HttpContext.Response.Cookies.Append(key, cookie, option));
					await _databaseService.AddUnregisteredUser(cookie);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error occured while updating cookie or updating user: " + ex.Message);
				}
			}
		}

		public async Task<string> ParseCookie(string key)
		{
			return await Task.FromResult(_httpContextAccessor.HttpContext.Request.Cookies[key]);
		}

		public void RemoveCookie(string key)
		{
			_httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
		}
	}
}
