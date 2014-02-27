using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TermsAndConditionsViewModel : BaseViewModel
	{
		private string _termsAndConditions;
        public string TermsAndConditions 
		{ 
            get 
            { 
				if (_termsAndConditions.IsNullOrEmpty())
				{
				    _termsAndConditions = await this.Services().Terms.GetText();
                }
				return @_termsAndConditions; 
            } 
        }
	}
}

