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

			questions.Add(new AccountChargeQuestion { Question = "Question 1" });
			questions.Add(new AccountChargeQuestion { Question = "Question 2" });
			questions.Add(new AccountChargeQuestion { Question = "Question 3" });
			questions.Add(new AccountChargeQuestion { Question = "" });
			questions.Add(new AccountChargeQuestion { Question = "Question 5" });
			questions.Add(new AccountChargeQuestion { Question = "" });
			questions.Add(new AccountChargeQuestion { Question = "Question 7" });
			questions.Add(new AccountChargeQuestion { Question = "Question 8" });

			tcs.TrySetResult (questions.ToArray ());

			return tcs.Task;
		}
	}
}