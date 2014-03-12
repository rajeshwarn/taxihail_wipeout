using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class TermsAndConditionsViewModel : PageViewModel
	{
		private readonly ITermsAndConditionsService _termsService;

		public TermsAndConditionsViewModel(ITermsAndConditionsService termsService)
		{
			_termsService = termsService;
		}

        public override async void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);
            using (this.Services().Message.ShowProgressNonModal())
            {
                var terms = await _termsService.GetTerms();
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

