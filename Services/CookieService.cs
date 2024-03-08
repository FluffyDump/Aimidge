namespace Aimidge.Services
{
	public class CookieService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CookieService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task SetCookie()
		{
			string key = "Cookie";
			string value = Guid.NewGuid().ToString();

			var option = new CookieOptions();

			option.Expires = DateTime.Now.AddMinutes(20);
			option.Secure = true;
			option.HttpOnly = true;
			option.SameSite = SameSiteMode.Strict;

			await Task.Run(() => _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, option));
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
