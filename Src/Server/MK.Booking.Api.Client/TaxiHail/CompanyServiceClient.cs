using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using MK.Common.Configuration;
using MK.Common.Exceptions;

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

        private TermsAndConditions HandleException(Exception error)
        {
            var webServiceError = error as WebServiceException;

            if (webServiceError == null || webServiceError.StatusCode != (int) HttpStatusCode.NotModified)
            {
                return null;
            }

            //get the object from the cache and deserialize
            var terms = _cacheService.Get<TermsAndConditions>("Terms");
            terms.Updated = false;
            return terms;
        }

        private void HandleResponseHeader(HttpResponseMessage response)
        {
            var version = response.Headers.ETag;
            if (version != null && version.Tag.HasValueTrimmed())
            {
                //put in the cache the etag
                _cacheService.Set("TermsVersion", version.Tag);
            }
        }

        private void AddVersionInformation()
        {
            //get the etag from the cache and add it to the headers
            var version = _cacheService.Get<string>("TermsVersion");
            if (version != null)
            {
                Client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(version));
            }
        }

        public async Task<TermsAndConditions> GetTermsAndConditions()
        {
            try
            {
                AddVersionInformation();

                return await Client.GetAsync<TermsAndConditions>("/termsandconditions", HandleResponseHeader);
            }
            catch (Exception ex)
            {
                var result = HandleException(ex);

                if (result == null)
                {
                    throw;
                }

                return result;
            }
        }
			
        public Task<AccountCharge> GetAccountCharge(string accountNumber, string customerNumber)
        {
#if CLIENT
                const bool hideAnswers = true;
#else
                const bool hideAnswers = false;
#endif

            var request = string.Format("/admin/accountscharge/{0}/{1}/{2}", accountNumber, customerNumber, hideAnswers);
            return Client.GetAsync<AccountCharge>(request);
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