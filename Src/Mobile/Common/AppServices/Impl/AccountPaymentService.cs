using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class AccountPaymentService : BaseService, IAccountPaymentService
	{
		public Task<AccountCharge> GetAccountCharge(string accountNumber, string customerNumber)
		{
            return UseServiceClientAsync<CompanyServiceClient, AccountCharge>(service => service.GetAccountCharge(accountNumber, customerNumber));
		}

        public async Task<AccountChargeQuestion[]> GetQuestions(string accountNumber, string customerNumber)
		{
            var response = await GetAccountCharge(accountNumber, customerNumber);
			return response.Questions;
		}
	}
}