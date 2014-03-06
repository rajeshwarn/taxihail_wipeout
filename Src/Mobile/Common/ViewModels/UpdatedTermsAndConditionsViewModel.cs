using apcurium.MK.Booking.Mobile.Extensions;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class UpdatedTermsAndConditionsViewModel : BaseSubViewModel<bool>
	{
		public override async void OnViewStarted(bool firstTime)
		{
			TermsAndConditions = "- Ipsum thingy -";
				var terms = await this.Services().Terms.GetTerms();
			    TermsAndConditions = terms.Content;
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
