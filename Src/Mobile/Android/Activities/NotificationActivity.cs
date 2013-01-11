using System;
using Android.App;
using System.Linq;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Collections.Generic;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Client.Activities;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Activity]
	public class NotificationActivity : BaseBindingActivity<NotificationViewModel>
	{

		protected override int ViewTitleResourceId {
			get {
				return 0;
			}
		}
		protected override void OnViewModelSet ()
		{

		}

	}
}

