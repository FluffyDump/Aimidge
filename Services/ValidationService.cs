using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Aimidge.Services
{
    public class ValidationService
    {
        public static string CheckProfanity(string prompt)
        {
            string[] prompt_parts = prompt.Split(' ');
            string[] badWords = File.ReadAllLines("Profanity.txt");
			string pattern = @"\b\p{L}*?(\p{L})\1{2,}\p{L}*\b|(?=.*[A-Za-z])(?=.*\d)";
			if (string.IsNullOrWhiteSpace(prompt)) { return prompt; }

			foreach (string word in prompt_parts)
			{
				if (Regex.Match(word, pattern).Success)
					return word;
			}


			foreach (string word in prompt_parts)
                {
                if (badWords.Contains(word.ToLower()))
                {
                    return word;
                }
                }
            return "ok";
        }

        public async Task<bool> CheckPrompt(string prompt)
        {
            string apiUrl = "http://193.161.193.99:61464/check_prompt";

            string json = @"{""prompt"": """ + prompt + @"""}";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("translatePrompt", "application/json");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
                    bool isEnglish = responseObject.is_english;
                    return isEnglish;
                }
            }
            return false;
        }
    }
}
