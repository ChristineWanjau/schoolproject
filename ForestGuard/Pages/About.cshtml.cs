using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;


namespace ForestGuard.Pages
{
    public class AboutModel : PageModel
    {

        public string? responseContent { get; set; }

        public class Sensor
        {
            public string? deviceId { get; set; }

            public string? generationId { get; set; }

            public string? eTag { get; set; }

            public string? connectionState { get; set; }

            public string? statusUpdatedTime { get; set; }

            public int? cloudToDeviceMessageCount { get; set; }

        
        }

        public List<Sensor>? Result { get; set; }
            public async Task OnGetAsync()
        {
            HttpClient client = new HttpClient();

            Uri baseURL = new Uri("https://ForestGuardHub.azure-devices.net/devices?api-version=2020-05-31-preview");

            // Any parameters? Get value, and then add to the client 
            var sasToken = generateSasToken("ForestGuardHub.azure-devices.net/devices", "TG/MUc52slMFZXo/ul3Ntqt+DMKtbN1ODOs+/uemvUk=", "registryRead");
       
            client.DefaultRequestHeaders.Add("Authorization",sasToken);

            HttpResponseMessage response = await client.GetAsync(baseURL.ToString());


            responseContent = await response.Content.ReadAsStringAsync();
            Result = System.Text.Json.JsonSerializer.Deserialize<List<Sensor>>(responseContent);
            
        }

        public static string generateSasToken(string resourceUri, string key, string policyName, int expiryInSeconds = 3600)
        {
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + expiryInSeconds);

            string stringToSign = WebUtility.UrlEncode(resourceUri) + "\n" + expiry;

            HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(key));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            string token = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}", WebUtility.UrlEncode(resourceUri), WebUtility.UrlEncode(signature), expiry);

            if (!String.IsNullOrEmpty(policyName))
            {
                token += "&skn=" + policyName;
            }

            return token;
        }
    }

}
