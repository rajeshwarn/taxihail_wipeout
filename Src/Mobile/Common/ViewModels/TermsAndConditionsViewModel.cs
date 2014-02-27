using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TermsAndConditionsViewModel : BaseViewModel
	{
		public async void Init()
		{
			TermsAndConditions = await this.Services().Terms.GetText();
		}
				
		public string TermsAndConditions { get; set; }
	}
}

