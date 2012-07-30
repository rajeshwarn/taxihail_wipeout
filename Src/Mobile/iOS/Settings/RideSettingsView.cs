using System;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.Framework;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class RideSettingsView : DialogViewController
	{
		public event EventHandler Closed;

		private BookingSettings _settings;
		private EntryElement _nameEntry;
		private EntryElement _phoneEntry;
		private EntryElement _passengerEntry;
		private RootElement _vehiculeTypeEntry;
		private EntryElement _numberOfTaxiEntry;
		
		private int _selected = 0;
		private bool _autoSave;
		private bool _companyOnly;

		public RideSettingsView (BookingSettings settings, bool autoSave, bool companyOnly) : base(null)
		{
			_companyOnly = companyOnly;
			_autoSave = autoSave;
			_settings = settings.Copy ();
		}

		public BookingSettings Result {
			get { return _settings; }
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			var button = new MonoTouch.UIKit.UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
				CloseView (); });
			NavigationItem.HidesBackButton = true;
			NavigationItem.RightBarButtonItem = button;
			
		}

		protected override void SetTitleView ()
		{
			NavigationItem.TitleView = AppContext.Current.Controller.GetTitleView (null, GetTitle ());
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
			_numberOfTaxiEntry.Maybe (() => _numberOfTaxiEntry.FetchValue ());
			_passengerEntry.Maybe (() => _passengerEntry.FetchValue ());
			
			
			if (Closed != null)
			{
				Closed (this, EventArgs.Empty);
			}
			
			
			if (!_companyOnly)
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
                    TinyIoCContainer.Current.Resolve<IAccountService>().UpdateSettings( _settings );
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
					var companiesList = TinyIoCContainer.Current.Resolve<IAccountService> ().GetCompaniesList ();
					index = 0;
					selected = 0;
					foreach (ListItem company in companiesList)
					{
						if (!company.Display.ToSafeString ().ToLower ().Contains ("test"))
						{
						
							var item = new RadioElement (company.Display);
							item.ItemId = company.Id;
							item.Tapped += delegate {
								SetCompany (item); };
							companies.Add (item);
							if (_settings.ProviderId == company.Id)
							{
								selected = index;
							}
							index++;
						}
					}
					
					var companyEntry = new RootElement (Resources.RideSettingsCompany, new RadioGroup (selected));
					companyEntry.Add (companies);
					
					
					
					
					this.InvokeOnMainThread (() => {
						this.Root = companyEntry; });
					
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
					_nameEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.Default;
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
					
					_phoneEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.PhonePad;
					
					
					if (AppSettings.ShowNumberOfTaxi)
					{
						_numberOfTaxiEntry = new EntryElement (Resources.RideSettingsNumberOfTaxi, "", _settings.NumberOfTaxi.ToString ());
						_numberOfTaxiEntry.Changed += delegate {
							_numberOfTaxiEntry.FetchValue ();
							int r;
							if (int.TryParse (_numberOfTaxiEntry.Value, out r))
							{
								_settings.NumberOfTaxi = r;
							}
							else
							{
								_settings.NumberOfTaxi = 1;
							}
							ApplyChanges ();
						};
					
						_numberOfTaxiEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.NumberPad;
					}




					_passengerEntry = new EntryElement (Resources.RideSettingsPassengers, "", _settings.Passengers.ToString ());
					_passengerEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.NumberPad;
					
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
					
					var vehicules = TinyIoCContainer.Current.Resolve<IAccountService> ().GetVehiclesList ();
					
					int index = 0;
					
					
					foreach (ListItem vType in vehicules)
					{
						var item = new RadioElement (vType.Display);
						item.ItemId = vType.Id;
						item.Tapped += delegate {
							
								_selected = _vehiculeTypeEntry.Sections[0].Elements.IndexOf( item );
								SetVehiculeType (item);
							
						};
						vehiculeTypes.Add (item);
						if (_settings.VehicleTypeId == vType.Id)
						{
							_selected = index;
						}
						index++;
					}
					_vehiculeTypeEntry = new RootElement (Resources.RideSettingsVehiculeType, new RadioGroup (_selected));
					_vehiculeTypeEntry.Add (vehiculeTypes);
				
					
					//var companyEntry = new StringElement ( Resources.RideSettingsCompany,  _settings.CompanyName );
					
					
					
					
//					var companies = new Section (Resources.RideSettingsCompany);
//					var companiesList = TinyIoCContainer.Current.Resolve<IAccountService> ().GetCompaniesList ();
//					index = 0;
//					selected = 0;
//					foreach (ListItem company in companiesList)
//					{
//						//if (!company.Display.ToSafeString ().ToLower ().Contains ("test")) {
//						
//						var item = new RadioElement (company.Display);
//						item.ItemId = company.Id;
//						item.Tapped += delegate {
//							SetCompany (item); };
//						companies.Add (item);
//						if (_settings.Company == company.Id)
//						{
//							selected = index;
//						}
//						index++;
//						//}
//					}
//					
//					var companyEntry = new RootElement (Resources.RideSettingsCompany, new RadioGroup (selected));
//					companyEntry.Add (companies);
//					
					
					
					
					var chargeTypes = new Section (Resources.RideSettingsChargeType);
					
					var payements = TinyIoCContainer.Current.Resolve<IAccountService> ().GetPaymentsList ();
					
					index = 0;
					int selected = 0;
					
					
					foreach (ListItem pay in payements)
					{
						var item = new RadioElement (pay.Display);
						item.ItemId = pay.Id;
						item.Tapped += delegate {
							SetChargeType (item); };
						chargeTypes.Add (item);
						if (_settings.ChargeTypeId == pay.Id)
						{
							selected = index;
						}
						index++;
					}
					var chargeTypeEntry = new RootElement (Resources.RideSettingsChargeType, new RadioGroup (selected));
					chargeTypeEntry.Add (chargeTypes);

					//_exceptionsEntry = CreateExceptionsSection(); 

					menu.Add (settings);
					
					settings.Add (_nameEntry);
					settings.Add (_phoneEntry);
					
					settings.Add (_passengerEntry);
					settings.Add (_vehiculeTypeEntry);
				//	settings.Add (companyEntry);
					settings.Add (chargeTypeEntry);
					
					if (AppSettings.ShowNumberOfTaxi)
					{
						settings.Add (_numberOfTaxiEntry);
					}

					//settings.Add( _exceptionsEntry );
					
					
					this.InvokeOnMainThread (() => {
						this.Root = menu; });
					
				}
				finally
				{
					
				}
				
			});
			
		}

		private void SetVehiculeType (RadioElement item)
		{
			SetVehiculeType( item.ItemId, item.Caption );
		}

		private void SetVehiculeType (int id, string vehiculeName )
		{
			_settings.VehicleTypeId = id;			
			ApplyChanges ();
			
		}

		private void SetChargeType (RadioElement item)
		{
			_settings.ChargeTypeId = item.ItemId;			
			ApplyChanges ();
			
		}

		private void SetCompany (RadioElement item)
		{
			_settings.ProviderId = item.ItemId;			
			ApplyChanges ();
		}

		private void SetException( CheckboxElement item )
		{
			//_settings.Exceptions.Single( e => e.Id == item.ItemId ).Value = !_settings.Exceptions.Single( e => e.Id == item.ItemId ).Value;
			ApplyChanges ();
		}
	
	}
}

