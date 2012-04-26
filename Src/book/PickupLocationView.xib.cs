using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;

namespace TaxiMobileApp
{
	public partial class PickupLocationView : UIViewController
	{



		private AddressContoller _addressController;
		private BookTabView _parent;

		#region Constructors

		public PickupLocationView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public PickupLocationView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public PickupLocationView (BookTabView parent) : base("PickupLocationView", null)
		{
			_parent = parent;
			_parent.TabSelected += TabSelected;
			Initialize ();
		}



		void Initialize ()
		{
		}


		private UIView _txtReadonlyView;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			btnPickAddress.Hidden = false;
			
			imageFieldBackground.Image = UIImage.FromFile ("Assets/TopFieldBackground-Lg.png");
			
			txtAptSuite.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
			txtRingCode.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
			txtAddress.Placeholder = Resources.PickupTextPlaceholder;
			txtAptSuite.Placeholder = Resources.AptNumberTextPlaceholder;
			
			txtTime.Placeholder = Resources.PickupDateTextPlaceholder + "  -  " + Resources.PickupTimeTextPlaceholder;
			txtRingCode.Placeholder = Resources.RingCodeTextPlaceholder;
			
			
			txtTime.EditingDidBegin += delegate(object sender, EventArgs e) {
				
				if (_txtReadonlyView != null) {
					_txtReadonlyView.RemoveFromSuperview ();
					_txtReadonlyView.Dispose ();
					_txtReadonlyView = null;
				}
				
				_txtReadonlyView = new UIView { Frame = txtTime.Frame, BackgroundColor = UIColor.Clear };
				this.View.AddSubview (_txtReadonlyView);
				
				if (_picker != null) {
					if (_parent.BookingInfo.PickupDate.HasValue) {
						_picker.SetDate (DateTimeToNSDate (_parent.BookingInfo.PickupDate.Value.ToUniversalTime ()), true);
					
					} else {
						_picker.SetDate (DateTimeToNSDate (DateTime.Now.ToUniversalTime ().AddMinutes (15)), true);
					}
				}
			};
			
			txtTime.EditingDidEnd += delegate(object sender, EventArgs e) {
				
				if (_txtReadonlyView != null) {
					_txtReadonlyView.RemoveFromSuperview ();
					_txtReadonlyView.Dispose ();
					_txtReadonlyView = null;
				}
			};
			
			
			txtTime.InputView = GetDatePicker ();
			
			
			lblPickup.Text = Resources.PickupViewPickupLabel;
			lblDestination.Text = Resources.PickupViewDestinationLabel;
			
			mapPickUp.Delegate = new AddressMapDelegate ();
			
			
			_addressController = new AddressContoller (txtAddress, txtAptSuite, txtRingCode, tableAddress, btnPickAddress, mapPickUp, AddressAnnotationType.Pickup, Resources.PickupMapTitle, 
			                                           () => _parent.BookingInfo.PickupLocation, data => _parent.BookingInfo.PickupLocation = data, ()=> _parent.IsTopView  );
			
			
			
			txtRingCode.ShouldReturn = delegate(UITextField textField) { return textField.ResignFirstResponder (); };
			txtAptSuite.ShouldReturn = delegate(UITextField textField) { return textField.ResignFirstResponder (); };
			
			txtAptSuite.EditingChanged += delegate { _parent.BookingInfo.PickupLocation.Apartment = txtAptSuite.Text; };
			txtRingCode.EditingChanged += delegate { _parent.BookingInfo.PickupLocation.RingCode = txtRingCode.Text; };
			
			_addressController.ShowCurrentLocation (AppContext.Current.LoggedUser != null);
			
			
			
			mapPickUp.ShowsUserLocation = true;
			
		}

		public void Display ()
		{
			
		}
		
		public override void ViewDidUnload ()
		{
			Console.WriteLine( "-------------VIEW UNLOADED-------------" );
			
		}

		public static NSDate DateTimeToNSDate (DateTime date)
		{
			return NSDate.FromTimeIntervalSinceReferenceDate ((date - (new DateTime (2001, 1, 1, 0, 0, 0))).TotalSeconds);
		}

		public void RefreshData ()
		{
			if ( !AppContext.Current.LastOrder.HasValue  )
			{			
				_addressController.ShowCurrentLocation (AppContext.Current.LoggedUser != null);
			}
			
		}

		private UIDatePicker _picker;
		private UIView GetDatePicker ()
		{
			_picker = new UIDatePicker ();
			UIView view = new UIView ();
			
			view.Frame = new RectangleF (0, 0, 320, 260);
			
			view.BackgroundColor = UIColor.Gray;
			
			var accept = UIButton.FromType (UIButtonType.RoundedRect);
			accept.Frame = new RectangleF (40, 5, 100, 35);
			accept.SetTitle (Resources.Close, UIControlState.Normal);
			view.AddSubview (accept);
			GlassButton.Wrap (accept, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor).TouchUpInside += delegate { txtTime.ResignFirstResponder (); };
			
			
			
			
			var reset = UIButton.FromType (UIButtonType.RoundedRect);
			reset.Frame = new RectangleF (180, 5, 100, 35);
			reset.SetTitle (Resources.Now, UIControlState.Normal);
			
			
			view.AddSubview (reset);
			GlassButton.Wrap (reset, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor).TouchUpInside += delegate {
				_parent.BookingInfo.PickupDate = null;
				txtTime.Text = "";
				txtTime.ResignFirstResponder ();
			};
			
			_picker.MinuteInterval = 15;
			_picker.Frame = new RectangleF (0, 45, 320, 300);
			view.AddSubview (_picker);
			
			_picker.ValueChanged += PickerValueChanged;
			
			return view;
		}

		void PickerValueChanged (object sender, EventArgs e)
		{
			if (_picker != null) {
				txtTime.Text = Resources.Date + " : " + NSDateToDateTime (_picker.Date).ToLocalTime ().ToShortDateString () + @"  -  " + Resources.Time + " : " + NSDateToDateTime (_picker.Date).ToLocalTime ().ToShortTimeString ();
				_parent.BookingInfo.PickupDate = NSDateToDateTime (_picker.Date).ToLocalTime ();
			}
		}

		public static DateTime NSDateToDateTime (NSDate date)
		{
			return (new DateTime (2001, 1, 1, 0, 0, 0)).AddSeconds (date.SecondsSinceReferenceDate);
		}


		public void PrepareData ()
		{
			_addressController.PerpareData ();
			
			if (!txtTime.Text.HasValue ()) {
				_parent.BookingInfo.PickupDate = null;
			}
			
		}

		public void SuspendRegionChanged ()
		{
			_addressController.SuspendRegionChanged ();
		}

		public void ResumeRegionChanged ()
		{
			_addressController.ResumeRegionChanged ();
		}

		public void AssignData ()
		{
			_addressController.AssignData ();
			
			
			if (_parent.BookingInfo.PickupDate.HasValue) {
				txtTime.Text = Resources.Date + " : " + _parent.BookingInfo.PickupDate.Value.ToShortDateString () + @"  -  " + Resources.Time + " : " + _parent.BookingInfo.PickupDate.Value.ToShortTimeString ();
			
			
			
			} else {
				txtTime.Text = "";
			}
			
		}


		void TabSelected (object sender, EventArgs e)
		{
			_addressController.Maybe (() => _addressController.ShowCurrentLocation (true));
		}
		
		
		
		
		#endregion
	}
}

