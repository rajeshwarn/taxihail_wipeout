using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using TaxiMobile.Helpers;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;
using TaxiMobile.ListViewCell;
using TaxiMobile.ListViewStructure;
using TaxiMobile.Models;

namespace TaxiMobile.Activities.Setting
{
	[Activity (Label = "RideSettingsActivity",  Theme = "@android:style/Theme.NoTitleBar", WindowSoftInputMode=SoftInput.AdjustPan, ScreenOrientation=ScreenOrientation.Portrait )]			
	public class RideSettingsActivity : ListActivity
	{
		private RideSettingsModel _model;
		private bool _updateDefaultSettings;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			BookingSetting currentSettings = null;
			var serializedSettings = Intent.GetStringExtra("BookingSettings");
			if( serializedSettings.HasValue() )
			{
				currentSettings = SerializerHelper.DeserializeObject<BookingSetting>( serializedSettings );
				_updateDefaultSettings = false;
			}
			else
			{
				currentSettings = AppContext.Current.LoggedUser.DefaultSettings;
				_updateDefaultSettings = true;
			}

            var service = ServiceLocator.Current.GetInstance<IStaticDataService>();
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
			section.AddItem( new SpinnerSectionItem( Resources.GetString( Resource.String.RideSettingsVehiculeType ), () => model.VehicleType, (value) => model.VehicleType = value, () => model.VehicleTypeList.Select( i => new ListItemData { Key = i.Id, Value = i.Display } ).ToList() ) );
			section.AddItem( new SpinnerSectionItem( Resources.GetString( Resource.String.RideSettingsCompany ), () => model.Company, (value) => model.Company = value, () => model.CompanyList.Select( i => new ListItemData { Key = i.Id, Value = i.Display } ).ToList() ) );
			section.AddItem( new SpinnerSectionItem( Resources.GetString( Resource.String.RideSettingsChargeType ), () => model.ChargeType, (value) => model.ChargeType = value, () => model.ChargeTypeList.Select( i => new ListItemData { Key = i.Id, Value = i.Display } ).ToList() ) );
			
			return structure;
		}
		
		private void UpdateDefaultRideSettings_Click(object sender, EventArgs e)
        {
			ThreadHelper.ExecuteInThread( this, () => { 
				if( _updateDefaultSettings )
				{

					var currentAccountData = AppContext.Current.LoggedUser;
					currentAccountData.DefaultSettings = _model.Data;
					currentAccountData.LastName = _model.Data.LastName;
					currentAccountData.FirstName = _model.Data.FirstName;
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
		
	}
}

