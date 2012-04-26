using System;
using Microsoft.Practices.ServiceLocation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;

namespace TaxiMobileApp
{
	public class RideSettingsView : DialogViewController
	{
		public event EventHandler Closed;

		private BookingSetting _settings;

		private EntryElement _nameEntry;
		private EntryElement _phoneEntry;
		private EntryElement _passengerEntry;
				
		//private EntryElement _numberOfTaxiEntry;
		

		private bool _autoSave;
		private bool _companyOnly;

		public RideSettingsView (BookingSetting settings, bool autoSave, bool companyOnly) : base(null)
		{
			_companyOnly = companyOnly;
			_autoSave = autoSave;
			_settings = settings.Copy ();
		}

		public BookingSetting Result {
			get { return _settings; }
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			var button = new UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate { CloseView (); });
			NavigationItem.HidesBackButton = true;
			NavigationItem.RightBarButtonItem = button;
			
		}


		protected override void SetTitleView ()
		{
			NavigationItem.TitleView = AppContext.Current.Controller.GetTitleView (null,GetTitle());
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (_companyOnly)
			{
				LoadSettingsElementsForComaganies ();
			}

			else
			{
				LoadSettingsElements ();
			}
			
		}

		private void CloseView ()
		{
			
			_nameEntry.Maybe (() => _nameEntry.FetchValue ());			
			_phoneEntry.Maybe (() => _phoneEntry.FetchValue ());			
			//_numberOfTaxiEntry.Maybe (()=> _numberOfTaxiEntry.FetchValue() );
			_passengerEntry.Maybe (() => _passengerEntry.FetchValue ());
			
			
			if (Closed != null)
			{
				Closed (this, EventArgs.Empty);
			}
			
			
			if ( !_companyOnly ) 
			{
				this.NavigationController.PopViewControllerAnimated (true);
			}
		}

		private void ApplyChanges ()
		{
			if (_autoSave)
			{
				ThreadHelper.ExecuteInThread (() =>
				{
					AppContext.Current.LoggedUser.DefaultSettings = _settings;
					AppContext.Current.UpdateLoggedInUser (AppContext.Current.LoggedUser, true);
				});
			}
			
		}

		private void LoadSettingsElementsForComaganies ()
		{
			
			
			ThreadHelper.ExecuteInThread (() =>
			{
				
				try
				{
					
					
					
					int index = 0;
					int selected = 0;
					
					var companies = new Section (Resources.RideSettingsCompany);
					var companiesList = ServiceLocator.Current.GetInstance<IAccountService> ().GetCompaniesList ();
					index = 0;
					selected = 0;
					foreach (ListItem company in companiesList)
					{
						//if (!company.Display.ToSafeString ().ToLower ().Contains ("test")) {
						
						var item = new RadioElement (company.Display);
						item.ItemId = company.Id;
						item.Tapped += delegate { SetCompany (item); };
						companies.Add (item);
						if (_settings.Company == company.Id)
						{
							selected = index;
						}
						index++;
						//}
					}
					
					var companyEntry = new RootElement (Resources.RideSettingsCompany, new RadioGroup (selected));
					companyEntry.Add (companies);
					
					
					
					
					this.InvokeOnMainThread (() => { this.Root = companyEntry; });
					
				}
				finally
				{
					
				}
				
			});
			
		}


		private void LoadSettingsElements ()
		{
			
			
			ThreadHelper.ExecuteInThread (() =>
			{
				
				try
				{
					
					
					var menu = new RootElement (this.Title);
					var settings = new Section (Resources.DefaultRideSettingsViewTitle);
					
					_nameEntry = new EntryElement (Resources.RideSettingsName, "", _settings.Name);
					_nameEntry.KeyboardType = UIKeyboardType.Default;
					_nameEntry.Changed += delegate {
						_nameEntry.FetchValue ();
						_settings.Name = _nameEntry.Value;
						ApplyChanges ();
					};
					
					_phoneEntry = new EntryElement (Resources.RideSettingsPhone, "", _settings.Phone);
					_phoneEntry.Changed += delegate {
						_phoneEntry.FetchValue ();
						_settings.Phone = _phoneEntry.Value;
						ApplyChanges ();
					};
					
					_phoneEntry.KeyboardType = UIKeyboardType.PhonePad;
					
					
//					_numberOfTaxiEntry = new EntryElement (Resources.RideSettingsNumberOfTaxi, "", _settings.NumberOfTaxi.ToString());
//					_numberOfTaxiEntry.Changed += delegate {
//						_numberOfTaxiEntry.FetchValue ();
//						int r;
//						if ( int.TryParse( _numberOfTaxiEntry.Value, out r ) )
//						{
//							_settings.NumberOfTaxi = r;
//						}
//						else{
//							_settings.NumberOfTaxi = 1;
//						}
//						ApplyChanges ();
//					};
//					
//					_numberOfTaxiEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.NumberPad;
					
					
					_passengerEntry = new EntryElement (Resources.RideSettingsPassengers, "", _settings.Passengers.ToString ());
					_passengerEntry.KeyboardType = UIKeyboardType.NumberPad;
					
					_passengerEntry.Changed += delegate {
						_passengerEntry.FetchValue ();
						int r;
						if (int.TryParse (_passengerEntry.Value, out r))
						{
							_settings.Passengers = r;
						}

						else
						{
							_passengerEntry.Value = "1";
						}
						ApplyChanges ();
						
					};
					
					var vehiculeTypes = new Section (Resources.RideSettingsVehiculeType);
					
					var vehicules = ServiceLocator.Current.GetInstance<IAccountService> ().GetVehiclesList ();
					
					int index = 0;
					int selected = 0;
					
					
					foreach (ListItem vType in vehicules)
					{
						var item = new RadioElement (vType.Display);
						item.ItemId = vType.Id;
						item.Tapped += delegate { SetVehiculeType (item); };
						vehiculeTypes.Add (item);
						if (_settings.VehicleType == vType.Id)
						{
							selected = index;
						}
						index++;
					}
					var vehiculeTypeEntry = new RootElement (Resources.RideSettingsVehiculeType, new RadioGroup (selected));
					vehiculeTypeEntry.Add (vehiculeTypes);
					
					var companies = new Section (Resources.RideSettingsCompany);
					var companiesList = ServiceLocator.Current.GetInstance<IAccountService> ().GetCompaniesList ();
					index = 0;
					selected = 0;
					foreach (ListItem company in companiesList)
					{
						//if (!company.Display.ToSafeString ().ToLower ().Contains ("test")) {
						
						var item = new RadioElement (company.Display);
						item.ItemId = company.Id;
						item.Tapped += delegate { SetCompany (item); };
						companies.Add (item);
						if (_settings.Company == company.Id)
						{
							selected = index;
						}
						index++;
						//}
					}
					
					var companyEntry = new RootElement (Resources.RideSettingsCompany, new RadioGroup (selected));
					companyEntry.Add (companies);
					
					
					
					
					var chargeTypes = new Section (Resources.RideSettingsChargeType);
					
					var payements = ServiceLocator.Current.GetInstance<IAccountService> ().GetPaymentsList ();
					
					index = 0;
					selected = 0;
					
					
					foreach (ListItem pay in payements)
					{
						var item = new RadioElement (pay.Display);
						item.ItemId = pay.Id;
						item.Tapped += delegate { SetChargeType (item); };
						chargeTypes.Add (item);
						if (_settings.ChargeType == pay.Id)
						{
							selected = index;
						}
						index++;
					}
					var chargeTypeEntry = new RootElement (Resources.RideSettingsChargeType, new RadioGroup (selected));
					chargeTypeEntry.Add (chargeTypes);
					
					
					menu.Add (settings);
					
					settings.Add (_nameEntry);
					settings.Add (_phoneEntry);
					
					settings.Add (_passengerEntry);
					settings.Add (vehiculeTypeEntry);
					settings.Add (companyEntry);
					settings.Add (chargeTypeEntry);
					
					//settings.Add (_numberOfTaxiEntry);
					
					
					
					this.InvokeOnMainThread (() => { this.Root = menu; });
					
				}
				finally
				{
					
				}
				
			});
			
		}

		private void SetVehiculeType (RadioElement item)
		{
			_settings.VehicleType = item.ItemId;
			_settings.VehicleTypeName = item.Caption;
			ApplyChanges ();
			
		}

		private void SetChargeType (RadioElement item)
		{
			_settings.ChargeType = item.ItemId;
			_settings.ChargeTypeName = item.Caption;
			ApplyChanges ();
			
		}



		private void SetCompany (RadioElement item)
		{
			_settings.Company = item.ItemId;
			_settings.CompanyName = item.Caption;
			ApplyChanges ();
		}
		
		
	}
}

