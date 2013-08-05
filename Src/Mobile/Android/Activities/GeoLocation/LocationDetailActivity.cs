using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Models;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation
{
    [Activity(Label = "Location Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LocationDetailActivity : BaseBindingActivity<LocationDetailViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_LocationDetail; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }

		protected override void OnViewModelSet ()
		{
			SetContentView(Resource.Layout.View_LocationDetail);
			FindViewById<EditText>(Resource.Id.LocationAddress).FocusChange += LocationDetailActivity_FocusChange;
		}


        void LocationDetailActivity_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
				ViewModel.ValidateAddress.Execute();
            }
        }
    }
}