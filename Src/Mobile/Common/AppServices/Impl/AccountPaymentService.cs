using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class AccountPaymentService : BaseService, IAccountPaymentService
	{
		public Task<AccountChargeQuestion[]> GetQuestions (string accountNumber)
		{
			var tcs = new TaskCompletionSource<AccountChargeQuestion[]>();

			var questions = new List<AccountChargeQuestion> ();

			questions.Add(new AccountChargeQuestion { Question = "What?" });
			questions.Add(new AccountChargeQuestion { Question = "Who?" });
			questions.Add(new AccountChargeQuestion { Question = "Where?" });
			questions.Add(new AccountChargeQuestion { Question = "" });
			questions.Add(new AccountChargeQuestion { Question = "When?" });
			questions.Add(new AccountChargeQuestion { Question = "" });
			questions.Add(new AccountChargeQuestion { Question = "Pow?" });
			questions.Add(new AccountChargeQuestion { Question = "Profit?" });

			tcs.TrySetResult (questions.ToArray ());

			return tcs.Task;
		}
	}
}