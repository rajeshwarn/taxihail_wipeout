using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using ServiceStack.Text;

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

		public async Task CheckIfNeedsToShowTerms(Action<object, Action<bool>> actionToDoIfTrue)
		{
			if (!_appSettings.Data.ShowTermsAndConditions)
			{
				return;
			}

			var response = await GetTerms();

			var ackKey = GetTermsAcknowledgmentKey (_accountService.CurrentAccount.Email);
			var termsAcknowledged = _cacheService.Get<string>(ackKey);

			if (response.Updated || !(termsAcknowledged == "yes"))
			{				
				_cacheService.Clear(ackKey);
				actionToDoIfTrue.Invoke(new 
					{
						content = response.Content
					}.ToStringDictionary(),
					async acknowledged =>
					{
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
			_cacheService.Set<string>(GetTermsAcknowledgmentKey(email), acknowledged ? "yes" : "no");
		}

		private string GetTermsAcknowledgmentKey(string email)
		{
			return string.Format("TermsAck{0}", email);
		}
    }
}

