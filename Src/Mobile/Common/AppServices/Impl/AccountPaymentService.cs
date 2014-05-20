using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class AccountPaymentService : BaseService, IAccountPaymentService
	{
		public Task<AccountPaymentQuestion[]> GetQuestions (string accountNumber)
		{
			var tcs = new TaskCompletionSource<AccountPaymentQuestion[]>();

			var questions = new List<AccountPaymentQuestion> ();

			questions.Add(new AccountPaymentQuestion { Question = "Question 1" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 2" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 3" });
			questions.Add(new AccountPaymentQuestion { Question = "" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 5" });
			questions.Add(new AccountPaymentQuestion { Question = "" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 7" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 8" });

			tcs.TrySetResult (questions.ToArray ());

			return tcs.Task;
		}
	}
}