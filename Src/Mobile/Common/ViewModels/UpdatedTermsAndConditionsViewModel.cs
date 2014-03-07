using apcurium.MK.Booking.Mobile.Extensions;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class UpdatedTermsAndConditionsViewModel : BaseSubViewModel<bool>
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
					ReturnResult(true);
				});
			}
		}
	}
}
