using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class OverlayService : IOverlayService
	{
		public OverlayService()
		{

		}

		public void StartAnimating()
		{
			LoadingOverlay.StartAnimatingLoading(  AppContext.Current.Controller.TopViewController.View, LoadingOverlayPosition.Center, null, 130, 30);
		}

		public void StopAnimating()
		{
			LoadingOverlay.StopAnimatingLoading( AppContext.Current.Controller.TopViewController.View );
		}

	}
}