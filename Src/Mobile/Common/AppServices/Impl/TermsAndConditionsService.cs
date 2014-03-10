using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Collections.Generic;

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

			var ackKey = GetTermsAcknowledgmentKey ();
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
						_cacheService.Set<string>(ackKey, acknowledged ? "yes" : "no");
						if (!acknowledged)
						{
							_accountService.SignOut();
						}
					});
			}
		}

		private string GetTermsAcknowledgmentKey()
		{
			return string.Format("TermsAck{0}", _accountService.CurrentAccount.Email);
		}
    }
}

