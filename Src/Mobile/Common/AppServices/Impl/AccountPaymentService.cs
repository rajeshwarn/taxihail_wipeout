using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class AccountPaymentService : BaseService, IAccountPaymentService
	{
		public Task<AccountCharge> GetAccountCharge(string accountNumber)
		{
			return UseServiceClientAsync<CompanyServiceClient, AccountCharge>(service => service.GetAccountCharge(accountNumber));
		}

		public async Task<AccountChargeQuestion[]> GetQuestions(string accountNumber)
		{
			var response = await GetAccountCharge (accountNumber);
			return response.Questions;
		}
	}
}