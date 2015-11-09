using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Animations;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Android.Views.InputMethods;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    /*
     * PARTIAL CLASS : the rest code is situated in the TaxiHail.Shared Project 
    */
    public partial class HomeActivity : BaseBindingFragmentActivity<HomeViewModel>, IChangePresentation
    {

	    protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _mainBundle = bundle;

			var errorCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(ApplicationContext);

            if (errorCode == ConnectionResult.ServiceMissing || errorCode == ConnectionResult.ServiceVersionUpdateRequired || errorCode == ConnectionResult.ServiceDisabled)
            {
				var dialog = GoogleApiAvailability.Instance.GetErrorDialog(this,errorCode, 0);
                dialog.Show();
                dialog.DismissEvent += (s, e) => Finish();
            }    
        }

    }
}