using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TinyIoC;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;
using apcurium.MK.Booking.Mobile.Client.ListViewCell;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "RideSettingsActivity", Theme = "@android:style/Theme.NoTitleBar", WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class RideSettingsActivity : BaseBindingActivity<RideSettingsViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_RideSettings; }
        }

		protected override void OnViewModelSet ()
		{
            SetContentView(Resource.Layout.View_RideSettings);
			var rideStructure = GetRideStructure(ViewModel);
			var listAdapter = new ListViewAdapter(this, rideStructure);
			//var listView = FindViewById<ListView>(Resource.Id.RideSettingsList);
			//listView.Adapter = listAdapter;
			//FindViewById<TextView>(Resource.Id.rideSettingsListHeader).Text = GetString(Resource.String.DefaultRideSettingsViewTitle);
		}

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
        }


        private ListStructure GetRideStructure(RideSettingsViewModel model)
        {
            var structure = new ListStructure(25, false);
            var section = structure.AddSection("Ride settings");

            section.AddItem(new TextEditSectionItem(Resources.GetString(Resource.String.RideSettingsName), () => model.Name, (value) => model.Name = value));
            section.AddItem(new TextEditSectionItem(Resources.GetString(Resource.String.RideSettingsPhone), () => model.Phone, (value) => model.Phone = value));
            section.AddItem(new TextEditSectionItem(Resources.GetString(Resource.String.RideSettingsPassengers), () => model.Passengers, (value) => model.Passengers = value));
            section.AddItem(new SpinnerSectionItem(Resources.GetString(Resource.String.RideSettingsVehiculeType), () => model.VehicleTypeId, (value) => model.VehicleTypeId = value, () => model.Vehicles.Select(i => new ListItemData { Key = i.Id, Value = i.Display }).ToList()));
            section.AddItem(new SpinnerSectionItem(Resources.GetString(Resource.String.RideSettingsChargeType), () => model.ChargeTypeId, (value) => model.ChargeTypeId = value, () => model.Payments.Select(i => new ListItemData { Key = i.Id, Value = i.Display }).ToList()));


            return structure;
        }

        

    }
}

