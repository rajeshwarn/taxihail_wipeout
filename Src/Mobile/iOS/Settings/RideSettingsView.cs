using System;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.Framework;
using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp
{
	public class RideSettingsView : DialogViewController
	{
		public event EventHandler Closed;

		private BookingSetting _settings;
		private EntryElement _nameEntry;
		private EntryElement _phoneEntry;
		private EntryElement _passengerEntry;
		private RootElement _vehiculeTypeEntry;
		private EntryElement _numberOfTaxiEntry;
		private RootElement _exceptionsEntry;
		private int _selected = 0;
		private bool _autoSave;
		private bool _companyOnly;
		private ListItem _vanType;

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
			var button = new MonoTouch.UIKit.UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
				CloseView (); });
			NavigationItem.HidesBackButton = true;
			NavigationItem.RightBarButtonItem = button;

			_vanType = ServiceLocator.Current.GetInstance<IAccountService> ().GetVehiclesList ().Single( vt => vt.Id == 171 );
			
		}

		protected override void SetTitleView ()
		{
			NavigationItem.TitleView = TaxiMobileApp.AppContext.Current.Controller.GetTitleView (null, GetTitle ());
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
						if (!company.Display.ToSafeString ().ToLower ().Contains ("test"))
						{
						
							var item = new RadioElement (company.Display);
							item.ItemId = company.Id;
							item.Tapped += delegate {
								SetCompany (item); };
							companies.Add (item);
							if (_settings.Company == company.Id)
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
							if( r > 4  && _settings.VehicleType != _vanType.Id )
							{
								SetVehiculeType( _vanType.Id, _vanType.Display );
								_vehiculeTypeEntry.RadioSelected = _vehiculeTypeEntry.Sections[0].Elements.IndexOf( _vehiculeTypeEntry.Sections[0].Elements.Single( e => ((RadioElement)e).ItemId ==_vanType.Id ));
								var root = _vehiculeTypeEntry.GetImmediateRootElement();
								root.Reload( _vehiculeTypeEntry, UITableViewRowAnimation.Fade );
								_selected = _vehiculeTypeEntry.Sections[0].Elements.IndexOf( _vehiculeTypeEntry.Sections[0].Elements.Single( e => ((RadioElement)e).ItemId ==_vanType.Id ) );
							}
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
					
					
					foreach (ListItem vType in vehicules)
					{
						var item = new RadioElement (vType.Display);
						item.ItemId = vType.Id;
						item.Tapped += delegate {
							if( _settings.Passengers > 4 && item.ItemId != _vanType.Id )
							{
								MessageHelper.Show( Resources.InvalidChoiceTitle, Resources.InvalidVehiculeTypeForNbPassenger, () => {
									_vehiculeTypeEntry.RadioSelected = _selected;
									var root = _vehiculeTypeEntry.GetImmediateRootElement();
									root.Reload( _vehiculeTypeEntry, UITableViewRowAnimation.Fade );

									var sectRoot = _vehiculeTypeEntry.Sections[0].Elements[ _vehiculeTypeEntry.Sections[0].Elements.IndexOf( item ) ].GetImmediateRootElement();
									sectRoot.Reload( _vehiculeTypeEntry.Sections[0].Elements[_vehiculeTypeEntry.Sections[0].Elements.IndexOf( item ) ], UITableViewRowAnimation.Fade );

									sectRoot = _vehiculeTypeEntry.Sections[0].Elements[_selected].GetImmediateRootElement();
									sectRoot.Reload( _vehiculeTypeEntry.Sections[0].Elements[_selected], UITableViewRowAnimation.Fade );
								});
							}
							else
							{
								_selected = _vehiculeTypeEntry.Sections[0].Elements.IndexOf( item );
								SetVehiculeType (item);
							}
						};
						vehiculeTypes.Add (item);
						if (_settings.VehicleType == vType.Id)
						{
							_selected = index;
						}
						index++;
					}
					_vehiculeTypeEntry = new RootElement (Resources.RideSettingsVehiculeType, new RadioGroup (_selected));
					_vehiculeTypeEntry.Add (vehiculeTypes);
				
					
					//var companyEntry = new StringElement ( Resources.RideSettingsCompany,  _settings.CompanyName );
					
					
					
					
//					var companies = new Section (Resources.RideSettingsCompany);
//					var companiesList = ServiceLocator.Current.GetInstance<IAccountService> ().GetCompaniesList ();
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
					
					var payements = ServiceLocator.Current.GetInstance<IAccountService> ().GetPaymentsList ();
					
					index = 0;
					int selected = 0;
					
					
					foreach (ListItem pay in payements)
					{
						var item = new RadioElement (pay.Display);
						item.ItemId = pay.Id;
						item.Tapped += delegate {
							SetChargeType (item); };
						chargeTypes.Add (item);
						if (_settings.ChargeType == pay.Id)
						{
							selected = index;
						}
						index++;
					}
					var chargeTypeEntry = new RootElement (Resources.RideSettingsChargeType, new RadioGroup (selected));
					chargeTypeEntry.Add (chargeTypes);

					_exceptionsEntry = CreateExceptionsSection(); 

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

					settings.Add( _exceptionsEntry );
					
					
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
			_settings.VehicleType = id;
			_settings.VehicleTypeName = vehiculeName;
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

		private void SetException( CheckboxElement item )
		{
			_settings.Exceptions.Single( e => e.Id == item.ItemId ).Value = !_settings.Exceptions.Single( e => e.Id == item.ItemId ).Value;
			ApplyChanges ();
		}

		private RootElement CreateExceptionsSection()
		{
			var exceptionsSection = new Section (Resources.Exceptions);

			foreach (ListItemValue exception in _settings.Exceptions )
			{
				var item = new CheckboxElement (exception.Display, exception.Value);
				item.ItemId = exception.Id;
				item.Tapped += delegate {
					SetException (item);
				};
				exceptionsSection.Add (item);
			}

			var exceptionsEntry = new RootElement ( Resources.Exceptions );
			exceptionsEntry.Add( exceptionsSection );

			return exceptionsEntry;
		}
		
		
	}
}

