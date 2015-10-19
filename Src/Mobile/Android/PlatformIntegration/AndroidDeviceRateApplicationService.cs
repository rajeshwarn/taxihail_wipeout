using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AndroidDeviceRateApplicationService:IDeviceRateApplicationService
	{
		/// <summary>
		/// http://developer.android.com/distribute/tools/promote/linking.html
		/// </summary>

		private static readonly string AndroidMarketLink = "market://details?id=";

		public void RedirectToRatingPage()
		{
			Android.Net.Uri androidMarketLink = Android.Net.Uri.Parse(AndroidMarketLink + Application.Context.PackageName);
			Intent androidMarketLinkIntent = new Intent(Intent.ActionView, androidMarketLink);
			androidMarketLinkIntent.SetFlags(ActivityFlags.NewTask);

			try
			{
				Application.Context.StartActivity(androidMarketLinkIntent);
			}
			catch (ActivityNotFoundException exception)
			{

			}
		}
	}
}