using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

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
            return @"http://cabmatedemo.drivelinq.com:8889/";
            //return _ibsSettings.WebServicesUrl;
        }

        private void SetupClient()
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(GetUrl()),
                // _ibsSettings.WebServicesUrl;
            };

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        protected T Get<T>(string pathInfo)
        {
            var dateStr = DateTime.Now.ToString("yyyy-MM-d hh:mm:ss");

            var stringToHash = "GET" + pathInfo + dateStr;
            var hash = Encode(stringToHash);

            Client.DefaultRequestHeaders.SetLoose("AUTHORIZATION", "EUGENE:{0}".FormatWith(hash));
            Client.DefaultRequestHeaders.SetLoose("DATE", dateStr);

            var response = Client.GetAsync(pathInfo).Result;

            Trace.WriteLine("**Request");
            Trace.WriteLine(response.RequestMessage);
            Trace.WriteLine("**Response");
            Trace.WriteLine(response);

            var resultJson = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Trace.WriteLine("Hash failed");
            }

            return resultJson.FromJson<T>();
        }

        private string Encode(string stringToHash)
        {

            HMAC hmac = new HMACSHA1("T!?_asF".ToBytes());

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

            Client.DefaultRequestHeaders.SetLoose("AUTHORIZATION", "EUGENE:{0}".FormatWith(hash));
            Client.DefaultRequestHeaders.SetLoose("DATE", dateStr);

            var response = Client.PostAsync(pathInfo, new StringContent(requestJson, Encoding.UTF8, "application/json")).Result;
            var resultJson = response.Content.ReadAsStringAsync().Result;

            Trace.WriteLine("**Request");
            Trace.WriteLine(response.RequestMessage);
            Trace.WriteLine(requestJson);
            Trace.WriteLine("**Response");
            Trace.WriteLine(response);
            Trace.WriteLine(resultJson);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Trace.WriteLine("Hash failed");
            }

            return resultJson.FromJson<T>();
        }

        protected ILogger Logger { get; set; }
    }
}
