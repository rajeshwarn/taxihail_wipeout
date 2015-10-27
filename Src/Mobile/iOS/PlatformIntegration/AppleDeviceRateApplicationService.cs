﻿using System;
using System.Collections.Generic;
using System.Text;
using apcurium.MK.Booking.Mobile.AppServices;
using UIKit;
using Foundation;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration;

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
        private IAppSettings _settings;

        public AppleDeviceRateApplicationService(IAppSettings settings)
        {
            _settings = settings;
        }

		public void RedirectToRatingPage()
		{
            NSUrl appleStoreLink = new Foundation.NSUrl(_settings.Data.AppleLink);

            UIApplication.SharedApplication.OpenUrl(appleStoreLink);

            appleStoreLink.Dispose();
            appleStoreLink = null;
		}
	}
}