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

using Microsoft.Practices.ServiceLocation;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;
using apcurium.MK.Booking.Mobile.Client.ListViewCell;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity (Label = "ChooseCompanyActivity",  Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]			
	public class ChooseCompanyActivity : ListActivity
	{
		private RideSettingsModel _model;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			var currentSettings = AppContext.Current.LoggedUser.DefaultSettings;
			var service = ServiceLocator.Current.GetInstance<IAccountService>();
			var companyList = service.GetCompaniesList();
//			var vehicleTypeList = service.GetVehiclesList();
//			var chargeTypeList = service.GetPaymentsList();
						
			_model = new RideSettingsModel( currentSettings, companyList, null, null );

			var companyStructure = GetCompanyStructure( _model );
			
			SetContentView( Resource.Layout.ChooseCompany );
			
			FindViewById<TextView>(Resource.Id.chooseCompanyListHeader).Text = GetString( Resource.String.ConfirmCompanyLabel );
			
			ListAdapter = new ListViewAdapter( this, companyStructure );
			
			FindViewById<Button>(Resource.Id.DoneBtn).Click += new EventHandler(UpdateDefaultRideSettings_Click);
		}
		
		
		private ListStructure GetCompanyStructure( RideSettingsModel model )
		{
			var structure = new ListStructure( 25, false );
			var section = structure.AddSection("Company list");
			
			foreach( var c in model.CompanyList )
			{
				section.AddItem( new BooleanSectionItem( c.Id, c.Display, (item) => item.Key == model.Company, value => model.Company = value ) );	               
			}
			

			return structure;
		}
		
		private void UpdateDefaultRideSettings_Click(object sender, EventArgs e)
        {
			AppContext.Current.LoggedUser.DefaultSettings.Company = _model.Company;
			AppContext.Current.LoggedUser.DefaultSettings.CompanyName = _model.CompanyName;
			Helpers.ThreadHelper.ExecuteInThread( Parent, () => {
				var currentAccountData = AppContext.Current.LoggedUser;
				currentAccountData.DefaultSettings = _model.Data;
				currentAccountData.Name = _model.Data.Name;
				AppContext.Current.UpdateLoggedInUser( currentAccountData, true );
			},false );
			Finish();
		}
		
	}
}

