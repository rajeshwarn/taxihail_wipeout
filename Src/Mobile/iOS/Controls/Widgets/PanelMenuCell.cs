using System;
using System.Drawing;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class PanelMenuCell : MvxStandardTableViewCell
	{
		public PanelMenuCell (string cellIdentifier, string bindingText)
			: base (bindingText, UITableViewCellStyle.Default, new NSString (cellIdentifier), UITableViewCellAccessory.None)
		{
			UIView customColorView =  new UIView();
			customColorView.BackgroundColor = UIColor.White;
			SelectedBackgroundView =  customColorView;

			Accessory = UITableViewCellAccessory.None;

			BackgroundColor = UIColor.Clear;

			TextLabel.Font = UIFont.FromName (FontName.HelveticaNeueLight, 36 / 2);
			TextLabel.TextColor = UIColor.FromRGB (79, 76, 71);
			TextLabel.BackgroundColor = UIColor.Clear;
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			TextLabel.SetX(5);
		}

	}
}

