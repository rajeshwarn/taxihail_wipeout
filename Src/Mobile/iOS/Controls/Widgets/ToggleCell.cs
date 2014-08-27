using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class ToggleCell : MvxStandardTableViewCell
	{
		private bool _hideBottomBar;
		private UISwitch _notificationToggle;

		public ToggleCell(IntPtr handle, string bindingText) : base(bindingText, handle)
		{
		}

		public ToggleCell (string cellIdentifier, string bindingText, UITableViewCellAccessory accessory) : base( bindingText, UITableViewCellStyle.Subtitle, new NSString(cellIdentifier), accessory  )
		{                   
			SelectionStyle = UITableViewCellSelectionStyle.None;
			Initialize ();
		}

		public bool HideBottomBar
		{
			get { return _hideBottomBar; }
			set
			{ 
				if (BackgroundView is CustomCellBackgroundView)
				{
					((CustomCellBackgroundView)BackgroundView).HideBottomBar = value;
				}
				_hideBottomBar = value;
			}
		}

		public string DisplayName 
		{ 
			get { return TextLabel.Text; }
			set { TextLabel.Text = Localize.GetValue(string.Format("Notification_{0}", value)); }
		}
			
		public bool Value
		{
			get { return _notificationToggle.On; }
			set { _notificationToggle.On = value; }
		}

		private void Initialize()
		{
			BackgroundView = new CustomCellBackgroundView(this.ContentView.Frame, 10, UIColor.White, UIColor.FromRGB(190, 190, 190)) 
			{
				HideBottomBar = HideBottomBar,              
			};

			ContentView.BackgroundColor = UIColor.Clear;
			BackgroundColor = UIColor.Clear;

			TextLabel.TextColor = UIColor.FromRGB(44, 44, 44);
			TextLabel.BackgroundColor = UIColor.Clear;
			TextLabel.Font = UIFont.FromName(FontName.HelveticaNeueBold, 28/2);
			this.TextLabel.TextAlignment ();

			ContentView.BackgroundColor = UIColor.Clear;
			BackgroundColor = UIColor.Clear;

			if (this.Accessory != UITableViewCellAccessory.None)
			{
				_notificationToggle = new UISwitch ();

				//_notificationToggle.

				// TODO: binding??

				AccessoryView = _notificationToggle;
			}
		}
	}
}

