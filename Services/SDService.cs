using Newtonsoft.Json;
using System.Text;

namespace Aimidge.Services
{
    public class SDService
    {

        public static string GetJsonPayLoad(string prompt, string width, string height)
        {

            string jsonPayload = @"
                {
                    ""prompt"": """ + prompt + @""",
                    ""steps"": 15,
                    ""width"": """ + width + @""",
                    ""height"": """ + height + @""",
                    ""restore_faces"": true,
                    ""save_images"": false
                }";
            return jsonPayload;
        }

        public static async Task<string> PostToAPIAsync(string jsonPayload)
        {

            string apiUrl = "http://193.161.193.99:61464/stable_diffusion";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("txt2img", "application/json");
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseData);
                    string base64Image = jsonResponse.images[0];
                    return base64Image ;
                }                
            }
            return "BadRequest";

        }


    }
}
