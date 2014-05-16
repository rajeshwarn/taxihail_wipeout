using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class AccountPaymentService : BaseService, IAccountPaymentService
	{
		public Task<AccountPaymentQuestion[]> GetQuestions (string accountNumber)
		{
			var questions = new List<AccountPaymentQuestion> ();

			questions.Add(new AccountPaymentQuestion { Question = "Question 1" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 2" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 3" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 4" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 5" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 6" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 7" });
			questions.Add(new AccountPaymentQuestion { Question = "Question 8" });

			return new Task<AccountPaymentQuestion[]>(() => questions.ToArray ());
		}
	}
}