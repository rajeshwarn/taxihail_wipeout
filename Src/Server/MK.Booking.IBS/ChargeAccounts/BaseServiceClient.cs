using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.IBS.ChargeAccounts
{
    public class BaseServiceClient
    {
        protected IBSSettingContainer _ibsSettings;

        protected BaseServiceClient(IBSSettingContainer ibsSettings, ILogger logger)
        {
            Logger = logger;
            _ibsSettings = ibsSettings;            
            SetupClient();
        }

        protected HttpClient Client { get; private set; }

        private string GetUrl()
        {
            var restApiUrl = _ibsSettings.RestApiUrl;
            if (!restApiUrl.HasValue())
            {
                throw new Exception("IBS RestApiUrl is not configured.  Configure it in the Company Settings before retrying.");
            }
            return restApiUrl;
        }

        private void SetupClient()
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(GetUrl()),
            };

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        protected T Get<T>(string pathInfo)
        {
            var dateStr = DateTime.Now.ToString("yyyy-MM-d hh:mm:ss");

            var stringToHash = "GET" + pathInfo + dateStr;
            var hash = Encode(stringToHash);

            Client.DefaultRequestHeaders.SetLoose("AUTHORIZATION", "{0}:{1}".FormatWith(_ibsSettings.RestApiUser, hash));
            Client.DefaultRequestHeaders.SetLoose("DATE", dateStr);

            var response = Client.GetAsync(pathInfo).Result;

            var resultJson = response.Content.ReadAsStringAsync().Result;

            return resultJson.FromJson<T>();
        }

        private string Encode(string stringToHash)
        {

            HMAC hmac = new HMACSHA1(_ibsSettings.RestApiSecret.ToBytes());

            var hash = hmac.ComputeHash(stringToHash.ToBytes());
            var str = hash.FromBytes();

            var encoded = hash.ToBase64String();

            return encoded;
        }

        protected T Post<T>(string pathInfo, object request)
        {
            var requestJson = request.ToJson();

            var dateStr = DateTime.Now.ToString("yyyy-MM-d hh:mm:ss");

            var stringToHash = "POST" + pathInfo + dateStr;
            var hash = Encode(stringToHash);

            Client.DefaultRequestHeaders.SetLoose("AUTHORIZATION", "{0}:{1}".FormatWith(_ibsSettings.RestApiUser, hash));
            Client.DefaultRequestHeaders.SetLoose("DATE", dateStr);

            var response = Client.PostAsync(pathInfo, new StringContent(requestJson, Encoding.UTF8, "application/json")).Result;
            var resultJson = response.Content.ReadAsStringAsync().Result;

            return resultJson.FromJson<T>();
        }

        protected ILogger Logger { get; set; }
    }
}
