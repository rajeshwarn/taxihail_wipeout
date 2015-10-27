using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class AccountChargeQuestionViewModel : BaseViewModel
	{
		public AccountChargeQuestionViewModel (AccountChargeQuestion model)
		{
			Model = model;
		}

		public AccountChargeQuestion Model {
			get;
			private set;
		}

		public string QuestionLabel
		{
			get
			{
				return string.Format ("{0} {1}",
					Model.Question,
					Model.IsRequired ? this.Services().Localize["AccountPaymentQuestionsRequired"] : string.Empty);
			}
		}

		public string QuestionPlaceholder
		{
			get{
				return Model.MaxLength.HasValue ? 
					string.Format (this.Services().Localize["AccountPaymentQuestionsPlaceHolder"], Model.MaxLength)
						: string.Empty;
			}
		}
	}
}

