using System;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MK.Common.Configuration;
using ServiceStack.Common.Web;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class CompanyServiceClient : BaseServiceClient
    {
        private readonly ICacheService _cacheService;

        public CompanyServiceClient(string url, string sessionId, IPackageInfo packageInfo, ICacheService cacheService)
            : base(url, sessionId, packageInfo)
        {
            _cacheService = cacheService;
        }

        private void HandleException(Exception error, TaskCompletionSource<TermsAndConditions> task)
        {
            var webServiceError = error as WebServiceException;
            if (webServiceError != null
                && webServiceError.StatusCode == (int)HttpStatusCode.NotModified)
            {
                //get the object from the cache and deserialize
                var terms = _cacheService.Get<TermsAndConditions>("Terms");
                terms.Updated = false;
                task.SetResult(terms);
            }
            else
            {
                task.SetException(error);
            }
        }

        private void HandleResponseHeader(HttpWebResponse response)
        {
            var version = response.Headers[HttpHeaders.ETag];
            if (version != null)
            {
                //put in the cache the etag
                _cacheService.Set("TermsVersion", version);
            }
        }

        private void AddVersionInformation(HttpWebRequest request)
        {
            //get the etag from the cache and add it to the headers
            var version = _cacheService.Get<string>("TermsVersion");
            if (version != null)
            {
                request.Headers.Set(HttpHeaders.IfNoneMatch, version);
            }
        }

        public Task<TermsAndConditions> GetTermsAndConditions()
        {
            var tcs = new TaskCompletionSource<TermsAndConditions>();

            Client.LocalHttpWebRequestFilter += AddVersionInformation;
            Client.LocalHttpWebResponseFilter += HandleResponseHeader;

            Client.GetAsync<string>("/termsandconditions",
                result =>
                {
                    var terms = result.FromJson<TermsAndConditions>();
                    _cacheService.Set("Terms", terms);
                    tcs.SetResult(terms);
                },
                (result, error) => HandleException(error, tcs));

            return tcs.Task;
        }
			
        public Task<AccountCharge> GetAccountCharge(string accountNumber)
        {
            var tcs = new TaskCompletionSource<AccountCharge>();
            bool hideAnswers = false;

            #if CLIENT
            hideAnswers = true;
            #endif

            string request = string.Format("/admin/accountscharge/{0}/{1}", accountNumber, hideAnswers);
            Client.GetAsync<AccountCharge>(request,
                tcs.SetResult,
                (result, error) => tcs.SetException(ServiceClientBaseExtensions.FixWebServiceException(error)));

            return tcs.Task;
        }

        public Task<NotificationSettings> GetNotificationSettings()
        {
            return Client.GetAsync<NotificationSettings>("/settings/notifications");
        }

        public Task<ActivePromotion[]> GetActivePromotions()
        {
            return Client.GetAsync(new ActivePromotions());
        }
    }
}