using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http.HttpResults;

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
                    httpClient.DefaultRequestHeaders.Add("unregistered", "application/json");
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

        public async Task<string> AddNewUser(string FirstName, string LastName, string Email, string PasswordHash)
        {
            string apiUrl = "http://193.161.193.99:61464/db_registration";
            try
            {
                string jsonPayload = @"
                {
                    ""firstName"": """ + FirstName + @""",
                    ""lastName"": """ + LastName + @""",
                    ""email"": """ + Email + @""",
                    ""passwordHash"": """ + PasswordHash + @"""
                }";

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("registered", "application/json");
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if(response.IsSuccessStatusCode)
                    {
                        return responseContent;
                    }
                    else
                    {
                        _logger.LogError($"Error occured in DatabaseService/AddNewUser: {response.StatusCode}");
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing the request: {ex}");
                return null;
            }
        }

        public async Task<string> AuthenticateUser(string Email, string PasswordHash)
        {
            string apiUrl = "http://193.161.193.99:61464/db_log_in";
            try
            {
                string jsonPayload = @"
                {
                    ""email"": """ + Email + @""",
                    ""passwordHash"": """ + PasswordHash + @"""
                }";

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("login", "application/json");
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if(response.IsSuccessStatusCode)
                    {
                        return responseContent;
                    }
                    else
                    {
                        _logger.LogError($"Error occured in DatabaseService/AddNewUser: {response.StatusCode}");
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing the request: {ex}");
                return null;
            }
        }
    }
}
