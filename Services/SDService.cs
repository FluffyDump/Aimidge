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
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return "403";
                }
            }
            return String.Empty;
        }

        public static async Task<bool> SaveImg(string uid)
        {
            string apiUrl = "http://193.161.193.99:61464/save_img";

            string json = @"{ ""uid"": """ + uid + @""" }";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("saveimg", "application/json");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static async Task<string> GetImg(string uid, string imgName)
        {
            string apiUrl = "http://193.161.193.99:61464/get_gallery_img";

            string json = @"
                {
                    ""uid"": """ + uid + @""",
                    ""img_name"": """ + imgName + @"""
                }";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("getImg", "application/json");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseData);
                    string base64Image = jsonResponse;
                    return base64Image ?? String.Empty;
                }
            }
            return String.Empty;
        }

        public static async Task<List<string>> GetImgNames(string uid)
        {
            string apiUrl = "http://193.161.193.99:61464/get_gallery_names";
            string json = @"{ ""uid"": """ + uid + @""" }";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("getImgCount", "application/json");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseData);
                    var imgNames = new List<string>();
                    foreach (var imgName in jsonResponse)
                    {
                        imgNames.Add(imgName.ToString());
                    }
                    return imgNames;
                }
            }
            return new List<string>();
        }

        public async Task<bool> RemoveImg(string uid, string imgName)
        {
            string apiUrl = "http://193.161.193.99:61464/remove_gallery_img";

            string json = @"
                {
                    ""uid"": """ + uid + @""",
                    ""img_name"": """ + imgName + @"""
                }";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("getImg", "application/json");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }
    }
}