using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using MK.Common.Configuration;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using MK.Common.Exceptions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class CompanyServiceClient : BaseServiceClient
    {
        private const string CacheKeyTerms = "Terms";
        private const string CacheKeyTermsVersion = "TermsVersion";

        private readonly ICacheService _cacheService;

        public CompanyServiceClient(string url, string sessionId, IPackageInfo packageInfo, ICacheService cacheService, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
            _cacheService = cacheService;
        }

        private void HandleResponseHeader(HttpResponseMessage response)
        {
			try 
			{
				var version = response.Headers.GetValues("ETag").FirstOrDefault();

				if (version.HasValueTrimmed())
				{
					//put in the cache the etag
					_cacheService.Set(CacheKeyTermsVersion, version);
				}
			}
			catch
			{
			    // ignored
			}
        }

        private void AddVersionInformation(HttpClient client)
        {
            //get the etag from the cache and add it to the headers
            var version = _cacheService.Get<string>(CacheKeyTermsVersion);
            if (version == null)
            {
                return;
            }

            if (client.DefaultRequestHeaders.Any(header => header.Key == "If-None-Match"))
            {
                client.DefaultRequestHeaders.Remove("If-None-Match");
            }

            client.DefaultRequestHeaders.TryAddWithoutValidation("If-None-Match", version);
        }

        public async Task<TermsAndConditions> GetTermsAndConditions()
        {
            try
            {
                TermsAndConditions termsAndConditions;

                using (var client = Client)
                {
                    AddVersionInformation(client);
                    termsAndConditions = await client.GetAsync<TermsAndConditions>("/termsandconditions", HandleResponseHeader);
                }

                _cacheService.Set(CacheKeyTerms, termsAndConditions);

                return termsAndConditions;
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
			
        private TermsAndConditions HandleException(Exception error)
        {
            var webServiceError = error as WebServiceException;

            if (webServiceError == null || webServiceError.StatusCode != (int) HttpStatusCode.NotModified)
            {
                return null;
            }

            //get the object from the cache and deserialize
            var terms = _cacheService.Get<TermsAndConditions>(CacheKeyTerms);

            if (terms == null)
            {
                return null;
            }

            terms.Updated = false;
            return terms;
        }

        public Task<AccountCharge> GetAccountCharge(string accountNumber, string customerNumber)
        {
#if CLIENT
                const bool hideAnswers = true;
#else
                const bool hideAnswers = false;
#endif

            var request = string.Format("/admin/accountscharge/{0}/{1}/{2}", accountNumber, customerNumber, hideAnswers);
            return Client.GetAsync<AccountCharge>(request, logger: Logger);
        }

        public Task<NotificationSettings> GetNotificationSettings()
        {
            return Client.GetAsync<NotificationSettings>("/settings/notifications", logger: Logger);
        }

        public Task<ActivePromotion[]> GetActivePromotions()
        {
            return Client.GetAsync(new ActivePromotions(), Logger);
        }
    }
}