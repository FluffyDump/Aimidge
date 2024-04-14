using Newtonsoft.Json;
using System.Text;

namespace Aimidge.Services
{
    public class SDService
    {

        private static string GetJsonPayLoad(string prompt, string width, string height, string uid)
        {
            string jsonPayload = @"
                {
                    ""uid"": """ + uid + @""",
                    ""prompt"": """ + prompt + @""",
                    ""steps"": 15,
                    ""width"": """ + width + @""",
                    ""height"": """ + height + @""",
                    ""restore_faces"": true,
                    ""save_images"": false
                }";
            return jsonPayload;
        }

        public static async Task<string> PostToAPIAsync(string prompt, string width, string height, string uid)
        {
            string apiUrl = "http://193.161.193.99:61464/stable_diffusion";

            string json = GetJsonPayLoad(prompt, width, height, uid);

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("txt2img", "application/json");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseData);
                    string base64Image = jsonResponse.images[0];
                    return base64Image ;
                }
            }
            return String.Empty;
        }
    }
}
