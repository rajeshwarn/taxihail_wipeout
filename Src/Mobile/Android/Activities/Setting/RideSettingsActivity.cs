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

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity (Label = "RideSettingsActivity",  Theme = "@android:style/Theme.NoTitleBar", WindowSoftInputMode=SoftInput.AdjustPan, ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]			
	public class RideSettingsActivity : ListActivity
	{
		private RideSettingsModel _model;
		private bool _updateDefaultSettings;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			BookingSettings currentSettings = null;
			var serializedSettings = Intent.GetStringExtra("BookingSettings");
			if( serializedSettings.HasValue() )
			{
				currentSettings = SerializerHelper.DeserializeObject<BookingSettings>( serializedSettings );
				_updateDefaultSettings = false;
			}
			else
			{
				currentSettings = AppContext.Current.LoggedUser.Settings;
				_updateDefaultSettings = true;
			}
			
			var service = TinyIoCContainer.Current.Resolve<IAccountService>();
			var companyList = service.GetCompaniesList();
			var vehicleTypeList = service.GetVehiclesList();
			var chargeTypeList = service.GetPaymentsList();
						
			_model = new RideSettingsModel( currentSettings, companyList, vehicleTypeList, chargeTypeList );

			var rideStructure = GetRideStructure( _model );
			
			SetContentView( Resource.Layout.RideSettings );
			
			FindViewById<TextView>(Resource.Id.rideSettingsListHeader).Text = GetString( Resource.String.DefaultRideSettingsViewTitle );
			
			ListAdapter = new ListViewAdapter( this, rideStructure );
			
			FindViewById<Button>(Resource.Id.DoneBtn).Click += new EventHandler(UpdateDefaultRideSettings_Click);
		}
		
		
		private ListStructure GetRideStructure( RideSettingsModel model )
		{
			var structure = new ListStructure( 25, false );
			var section = structure.AddSection("Ride settings");
			
			section.AddItem( new TextEditSectionItem( Resources.GetString( Resource.String.RideSettingsName ), () => model.Name, (value) => model.Name = value ) );
			section.AddItem( new TextEditSectionItem( Resources.GetString( Resource.String.RideSettingsPhone ), () => model.Phone, (value) => model.Phone = value ) );
			section.AddItem( new TextEditSectionItem( Resources.GetString( Resource.String.RideSettingsPassengers ), () => model.NbOfPassenger, (value) => model.NbOfPassenger = value ) );
			section.AddItem( new SpinnerSectionItem( Resources.GetString( Resource.String.RideSettingsVehiculeType ), () => model.VehicleTypeId, (value) => model.VehicleTypeId = value, () => model.VehicleTypeList.Select( i => new ListItemData { Key = i.Id, Value = i.Display } ).ToList() ) );
            section.AddItem(new SpinnerSectionItem(Resources.GetString(Resource.String.RideSettingsVehiculeType), () => model.ChargeTypeId, (value) => model.ChargeTypeId = value, () => model.ChargeTypeList.Select(i => new ListItemData { Key = i.Id, Value = i.Display }).ToList()));
            
            //TODO:Fix this
//			/section.AddItem( new SpinnerSectionItem( Resources.GetString( Resource.String.RideSettingsCompany ), () => model.Company, (value) => model.Company = value, () => model.CompanyList.Select( i => new ListItemData { Key = i.Id, Value = i.Display } ).ToList() ) );
	//		section.AddItem( new SpinnerSectionItem( Resources.GetString( Resource.String.RideSettingsChargeType ), () => model.ChargeTypeId, (value) => model.ChargeType = value, () => model.ChargeTypeList.Select( i => new ListItemData { Key = i.Id, Value = i.Display } ).ToList() ) );
			
			return structure;
		}
		
		private void UpdateDefaultRideSettings_Click(object sender, EventArgs e)
        {
		    if (ValidateRideSettings(_model.Data))
		    {
		        ThreadHelper.ExecuteInThread( this, () => { 
				if( _updateDefaultSettings )
				{

					var currentAccountData = AppContext.Current.LoggedUser;
					currentAccountData.Settings = _model.Data;
                    //TODO:Fix this
					//currentAccountData.FirstName = _model.Data.Name;
				    TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().UpdateBookingSettings(_model.Data);
					AppContext.Current.UpdateLoggedInUser( currentAccountData, true );

				}
				else
				{
					Intent i = new Intent();
					i.SetFlags(ActivityFlags.ForwardResult);
					i.PutExtra("BookingSettings", _model.Data.Serialize() );
					SetResult(Result.Ok, i );
				}
				
				Finish();
		
			}, true );
		    }
		    else
		    {
                this.ShowAlert(Resource.String.UpdateBookingSettingsInvalidDataTitle, Resource.String.UpdateBookingSettingsEmptyField);
		    }
			
		}

        public bool ValidateRideSettings(BookingSettings bookingSettings)
        {
            if ((string.IsNullOrEmpty(bookingSettings.Name)) || (string.IsNullOrEmpty(bookingSettings.Phone)) || (string.IsNullOrEmpty(bookingSettings.Passengers.ToString(CultureInfo.InvariantCulture))))
            {
                return false;
            }
            return true;
        }
		
	}
}

