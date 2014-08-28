using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.ComponentModel;
using Cirrious.MvvmCross.Binding.BindingContext;
using MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class ToggleCell : MvxStandardTableViewCell
	{
		private bool _hideBottomBar;
		public UISwitch NotificationToggle { get; set; }

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
			
		private void Initialize()
		{
			this.DelayBind (() => 
			{
				var set = this.CreateBindingSet<ToggleCell, ToggleItem> ();

				set.Bind (NotificationToggle)
					.For (v => v.On)
					.To (vm => vm.Value);

				set.Bind (TextLabel)
					.To (vm => vm.Display);

				set.Apply ();
			});

			BackgroundView = new CustomCellBackgroundView(this.ContentView.Frame, 0, UIColor.White, UIColor.FromRGB(190, 190, 190)) 
			{
				HideBottomBar = HideBottomBar,              
			};

			ContentView.BackgroundColor = UIColor.Clear;
			BackgroundColor = UIColor.Clear;

			TextLabel.TextColor = UIColor.FromRGB(44, 44, 44);
			TextLabel.BackgroundColor = UIColor.Clear;
			TextLabel.Font = UIFont.FromName(FontName.HelveticaNeueLight, 32 / 2);
			this.TextLabel.TextAlignment ();

			ContentView.BackgroundColor = UIColor.Clear;
			BackgroundColor = UIColor.Clear;

			if (this.Accessory != UITableViewCellAccessory.None)
			{
				NotificationToggle = new UISwitch ();

				AccessoryView = NotificationToggle;
			}
		}
	}
}

