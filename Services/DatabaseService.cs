using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace Aimidge.Services
{
    public class DatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseService(ILogger<DatabaseService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> AddUnregisteredUser(string uid)
        {
            string apiUrl = "http://193.161.193.99:61464/db_post_unregistered";
            try
            {
                DateTime tokenExpiration = DateTime.Now.AddMinutes(20);
                string formattedTokenExpiration = tokenExpiration.ToString("yyyy-MM-ddTHH:mm:ss");
                string jsonPayload = @"
                {
                    ""HasUploadedFiles"": true,
                    ""UserGuid"": """ + uid + @""",
                    ""TokenExpiration"": """ + formattedTokenExpiration + @"""
                }";

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("txt2img", "application/json");
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding user.");
                return false;
            }
        }
    }
}
