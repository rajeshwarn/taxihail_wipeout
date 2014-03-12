using apcurium.MK.Booking.Mobile.Extensions;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class UpdatedTermsAndConditionsViewModel : PageViewModel, ISubViewModel<bool>
	{
		public void Init(string content)
		{
			TermsAndConditions = content;
		}

		private string _termsAndConditions;
		public string TermsAndConditions
		{
			get { return _termsAndConditions; }
			set
			{
				_termsAndConditions = value;
				RaisePropertyChanged();
			}
		}

		public ICommand Confirm
		{
			get
			{
				return this.GetCommand(() =>
				{
					this.ReturnResult(true);
				});
			}
		}
	}
}
