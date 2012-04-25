
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using apcurium.Framework.Extensions;

namespace TaxiMobileApp
{
	public partial class ConfirmationView : UIViewController
	{
		private BookTabView _parent;

		public event EventHandler Confirmed;
		public event EventHandler Canceled;

		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public ConfirmationView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public ConfirmationView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public ConfirmationView (BookTabView parent) : base("ConfirmationView", null)
		{
			_parent = parent;
			Initialize ();
		}

		void Initialize ()
		{
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			NavigationItem.HidesBackButton=true;
			
			btnCancel.SetTitle (Resources.CancelBoutton, UIControlState.Normal);
			btnConfirm.SetTitle (Resources.ConfirmButton, UIControlState.Normal);
						
			lblOrigin.Text = Resources.ConfirmOriginLablel;
			lblAptRing.Text = Resources.ConfirmAptRingCodeLabel;
			lblDestination.Text = Resources.ConfirmDestinationLabel;
			lblDateTime.Text = Resources.ConfirmDateTimeLabel;
			lblName.Text = Resources.ConfirmNameLabel;
			lblPhone.Text = Resources.ConfirmPhoneLabel;
			lblPassengers.Text = Resources.ConfirmPassengersLabel;
			lblVehiculeType.Text = Resources.ConfirmVehiculeTypeLabel;
			lblCompany.Text = Resources.ConfirmCompanyLabel;
			lblDistance.Text = Resources.RideSettingsChargeType;
			lblPrice.Text = string.Format( Resources.EstimatePrice , "" );
			
			txtOrigin.Text = _parent.BookingInfo.PickupLocation.Address;
			txtAptRing.Text = FormatAptRingCode (_parent.BookingInfo.PickupLocation.Apartment, _parent.BookingInfo.PickupLocation.RingCode);			
			txtDestination.Text = _parent.BookingInfo.DestinationLocation.Address.HasValue () ? _parent.BookingInfo.DestinationLocation.Address : Resources.ConfirmDestinationNotSpecified;			
			txtDateTime.Text = FormatDateTime (_parent.BookingInfo.PickupDate, _parent.BookingInfo.PickupDate);
			
			txtDistance.Text = _parent.BookingInfo.Settings.ChargeTypeName;
			
			var distance = _parent.BookingInfo.GetDistance();
			var price = _parent.BookingInfo.GetPrice(distance);
			
			txtPrice.Text = price.HasValue ? string.Format ("{0:c}", price.Value) : Resources.NotAvailable;
			
			
			txtName.Text = _parent.BookingInfo.Settings.Name;
			txtPhone.Text = _parent.BookingInfo.Settings.Phone;
			txtPassengers.Text = _parent.BookingInfo.Settings.Passengers.ToString ();
			txtVehiculeType.Text = _parent.BookingInfo.Settings.VehicleTypeName;
			txtCompany.Text = _parent.BookingInfo.Settings.CompanyName;
			//btnCancel.TouchUpInside += CancelClicked;
			GlassButton.Wrap (btnCancel, AppStyle.CancelButtonColor, AppStyle.CancelButtonHighlightedColor).TouchUpInside +=CancelClicked;
				
			//btnConfirm.TouchUpInside += ConfirmClicked;
			GlassButton.Wrap (btnConfirm, AppStyle.AcceptButtonColor, AppStyle.AcceptButtonHighlightedColor).TouchUpInside +=ConfirmClicked;
						
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem( UIBarButtonSystemItem.Edit, delegate { EditRideSettings(); } );
			
					
		}
		
		public void RefreshCompany()
		{
		
			txtCompany.Text = _parent.BookingInfo.Settings.CompanyName;
		}
		
		public void EditRideSettings()
		{
			var settings = new  RideSettingsView ( _parent.BookingInfo.Settings , false, false);
			
			settings.Closed += delegate {
				_parent.BookingInfo.Settings = settings.Result;
				
				txtDistance.Text = _parent.BookingInfo.Settings.ChargeTypeName;
				txtName.Text = _parent.BookingInfo.Settings.Name;
				txtPhone.Text = _parent.BookingInfo.Settings.Phone;
				txtPassengers.Text = _parent.BookingInfo.Settings.Passengers.ToString ();
				txtVehiculeType.Text = _parent.BookingInfo.Settings.VehicleTypeName;
				txtCompany.Text = _parent.BookingInfo.Settings.CompanyName;						
				
			};
			
			this.NavigationController.PushViewController( settings , true );
		}
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			LoadLayout ();		
			
			
			if ( AppContext.Current.WarnEstimate )
			{
				MessageHelper.Show( Resources.WarningEstimateTitle,  Resources.WarningEstimate, Resources.WarningEstimateDontShow, ( ) => AppContext.Current.WarnEstimate = false  ); 
			}
		}

		private void LoadLayout ()
		{

			
			this.NavigationItem.TitleView = AppContext.Current.Controller.GetTitleView( null , Resources.ConfirmationViewTitle );
		}

		private string FormatDateTime (DateTime? pickupDate, DateTime? pickupTime)
		{
			string result = pickupDate.HasValue ? pickupDate.Value.ToShortDateString () : Resources.DateToday;
			result += @" / ";
			result += pickupTime.HasValue ? pickupTime.Value.ToShortTimeString () : Resources.TimeNow;
			return result;
		}
		private string FormatAptRingCode (string apt, string rCode)
		{
			
			string result = apt.HasValue () ? apt : Resources.ConfirmNoApt;
			result += @" / ";
			result += rCode.HasValue () ? rCode : Resources.ConfirmNoRingCode;
			return result;
		}
		void ConfirmClicked (object sender, EventArgs e)
		{
			
			if (Confirmed != null) {
				Confirmed (this, EventArgs.Empty);
			}
		}

		void CancelClicked (object sender, EventArgs e)
		{
			if (Canceled != null) {
				Canceled (this, EventArgs.Empty);
			}
		}
		
		
		public BookingInfoData BI
		{
			get { return _parent.BookingInfo; }
		}
		
		#endregion
	}
}

