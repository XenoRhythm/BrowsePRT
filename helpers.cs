using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace BrowsePRT
{
    

    public static class Helper
    {
        private sealed class NonceResponse
        {
            public string Nonce { get; set; }
        }

        public static string GetBrowserCoreFilepath()
        {
            List<string> filelocs = new List<string>()
            {
                @"C:\Program Files\Windows Security\BrowserCore\browsercore.exe",
                @"C:\Windows\BrowserCore\browsercore.exe"
            };

            return filelocs.FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException("Could not find browsercore.exe");
        }

        public static string RequestNonce()
        {
            string url = "https://login.microsoftonline.com/common/oauth2/token";
            string payload = "grant_type=srv_challenge";

            var content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
            using (var req = new HttpClient())
            {
                var response = req.PostAsync(url, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return JsonConvert.DeserializeObject<NonceResponse>(json).Nonce;
            }
        }
    }
}
