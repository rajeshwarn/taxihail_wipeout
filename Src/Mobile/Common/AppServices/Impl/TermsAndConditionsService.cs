using System;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common.Extensions;
using MK.Common.Exceptions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class TermsAndConditionsService : BaseService, ITermsAndConditionsService
    {
		private readonly IAppSettings _appSettings;
		private readonly IAccountService _accountService;
		private readonly ICacheService _cacheService;

		public TermsAndConditionsService(IAppSettings appSettings, IAccountService accountService, ICacheService cacheService)
		{
			_appSettings = appSettings;
			_accountService = accountService;
			_cacheService = cacheService;
		}

        public Task<TermsAndConditions> GetTerms()
        {
            return UseServiceClientAsync<CompanyServiceClient, TermsAndConditions>(service => service.GetTermsAndConditions());
        }

		public async Task CheckIfNeedsToShowTerms(Action<object, Action<bool>> actionToDoIfTrue, Action<bool, ZoomToStreetLevelPresentationHint> actionToDoOnReturn, bool initialLocateUserValue, ZoomToStreetLevelPresentationHint initialHintValue)
		{
			if (!_appSettings.Data.ShowTermsAndConditions)
			{
				return;
			}

			TermsAndConditions response = null;

		    try
		    {
		        response = await GetTerms();
		    }
		    catch (WebServiceException ex)
		    {
		        if (ex.StatusCode != (int)HttpStatusCode.NotModified)
		        {
		            Logger.LogError(ex);
		        }
		    }
		    catch(Exception ex)
		    {
		        Logger.LogError(ex);
		    }

			if (response == null) 
			{
				return;
			}

			var currentAccount = _accountService.CurrentAccount;
			if (currentAccount == null)
			{
				return;
			}

			var ackKey = GetTermsAcknowledgmentKey (currentAccount.Email);
			var termsAcknowledged = _cacheService.Get<string>(ackKey);

			if (response.Updated || termsAcknowledged != "yes")
			{				
				_cacheService.Clear(ackKey);
				actionToDoIfTrue.Invoke(new
				    {
				        content = response.Content.ToJson()
				    },
                    acknowledged =>
					{
						actionToDoOnReturn.Invoke(initialLocateUserValue, initialHintValue);
						AcknowledgeTerms(acknowledged, _accountService.CurrentAccount.Email);
						if (!acknowledged)
						{
							_accountService.SignOut();
						}
					});
			}
		}

		public void AcknowledgeTerms(bool acknowledged, string email)
		{
			_cacheService.Set(GetTermsAcknowledgmentKey(email), acknowledged ? "yes" : "no");
		}

		private string GetTermsAcknowledgmentKey(string email)
		{
			return string.Format("TermsAck{0}", email);
		}
    }
}

