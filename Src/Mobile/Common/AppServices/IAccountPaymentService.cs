using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IAccountPaymentService
	{
        Task<AccountCharge> GetAccountCharge(string accountNumber, string customerNumber);
        Task<AccountChargeQuestion[]> GetQuestions(string accountNumber, string customerNumber);
	}
}
