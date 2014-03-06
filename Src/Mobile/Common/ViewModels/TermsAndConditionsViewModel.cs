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
                var terms = await this.Services().Terms.GetTerms();
                TermsAndConditions = terms.Content;
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

