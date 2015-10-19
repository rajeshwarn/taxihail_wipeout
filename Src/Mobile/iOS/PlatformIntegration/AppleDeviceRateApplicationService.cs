using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{

	/// <summary>
	/// http://itunes.apple.com/linkmaker
	/// http://www.apple.com/itunes/linkmaker/faq/
	/// https://developer.apple.com/library/ios/qa/qa1629/_index.html
	/// https://developer.apple.com/library/ios/featuredarticles/iPhoneURLScheme_Reference/iPhoneURLScheme_Reference.pdf
	/// </summary>


	public class AppleDeviceRateApplicationService:IDeviceRateApplicationService
	{
		public void RedirectToRatingPage()
		{
			throw new NotImplementedException();
		}
	}
}