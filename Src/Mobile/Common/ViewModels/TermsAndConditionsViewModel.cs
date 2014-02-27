using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TermsAndConditionsViewModel : BaseViewModel
	{
        public override async void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);
            using (this.Services().Message.ShowProgressNonModal())
            {
                TermsAndConditions = await this.Services().Terms.GetText();
		    }
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
	}
}

