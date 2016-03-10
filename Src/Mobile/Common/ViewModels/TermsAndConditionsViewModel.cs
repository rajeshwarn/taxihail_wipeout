using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common;
using System.Net;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class TermsAndConditionsViewModel : PageViewModel
	{
		private readonly ITermsAndConditionsService _termsService;
		private readonly IConnectivityService _connectivityService;

		public TermsAndConditionsViewModel(ITermsAndConditionsService termsService, IConnectivityService connectivityService)
		{
			_termsService = termsService;
			_connectivityService = connectivityService;
		}

        public override async void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);
            using (this.Services().Message.ShowProgressNonModal())
            {
				try
				{
	                var terms = await _termsService.GetTerms();
	                TermsAndConditions = terms.Content;
				}
				catch(WebException e)
				{
					//Happens when the device is not connected
					_connectivityService.ShowToast();
				}
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

