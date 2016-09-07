using System;
using System.IO;
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
        protected IBSSettingContainer IbsSettings;

        protected BaseServiceClient(IBSSettingContainer ibsSettings, ILogger logger)
        {
            Logger = logger;
            IbsSettings = ibsSettings;            
            SetupClient();
        }

        protected HttpClient Client { get; private set; }

        private string GetUrl()
        {
            var restApiUrl = IbsSettings.RestApiUrl;
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
            pathInfo = pathInfo.StartsWith("/")
                ? pathInfo.Remove(0, 1)
                : pathInfo;
            
            var path = Path.Combine(Client.BaseAddress.AbsolutePath, pathInfo);
            
            AddAuthorizationTokenIfNeeded("GET", path);

            var response = Client.GetAsync(path).Result;

            var resultJson = response.Content.ReadAsStringAsync().Result;

            return resultJson.FromJson<T>();
        }

        void AddAuthorizationTokenIfNeeded(string httpMethod, string pathInfo)
        {
            if (!IbsSettings.RestApiUser.HasValueTrimmed() || !IbsSettings.RestApiSecret.HasValueTrimmed())
            {
                return;
            }

            var dateStr = DateTime.Now.ToString("yyyy-MM-d hh:mm:ss");

            var stringToHash = httpMethod + pathInfo + dateStr;
            var hash = Encode(stringToHash);

            Client.DefaultRequestHeaders.SetLoose("AUTHORIZATION", "{0}:{1}".FormatWith(IbsSettings.RestApiUser, hash));
            Client.DefaultRequestHeaders.SetLoose("DATE", dateStr);
        }

        private string Encode(string stringToHash)
        {
            var hmac = new HMACSHA1(IbsSettings.RestApiSecret.ToBytes());

            var hash = hmac.ComputeHash(stringToHash.ToBytes());
            hash.FromBytes();

            return hash.ToBase64String();
        }

        protected T Post<T>(string pathInfo, object request)
        {
            pathInfo = pathInfo.StartsWith("/")
                ? pathInfo.Remove(0, 1)
                : pathInfo;

            var path = Path.Combine(Client.BaseAddress.AbsolutePath, pathInfo);

            AddAuthorizationTokenIfNeeded("POST", path);

            var response = Client.PostAsync(path, new StringContent(request.ToJson(), Encoding.UTF8, "application/json")).Result;
            var resultJson = response.Content.ReadAsStringAsync().Result;

            return resultJson.FromJson<T>();
        }

        protected ILogger Logger { get; set; }
    }
}
