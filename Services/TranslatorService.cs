using Newtonsoft.Json;
using System.Text;

namespace Aimidge.Services
{
    public class TranslationService
    {
        public async Task<string> TranslatePrompt(string prompt)
        {
            string apiUrl = "http://193.161.193.99:61464/translate_prompt";

            string json = @"{""prompt"": """ + prompt + @"""}";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("translatePrompt", "application/json");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string translatedPrompt = responseObject.prompt;
                    return translatedPrompt;
                }
            }
            return String.Empty;
        }
    }
}
