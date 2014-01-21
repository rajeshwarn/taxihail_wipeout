using System;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.Foundation;
using System.Drawing;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class TwoLinesCell : MvxStandardTableViewCell
	{
		private UIImageView _arrowImage;
		private bool _showPlusSign;
		private bool _showArrow;
		private bool _hideBottomBar;

		private string _icon;
		public TwoLinesCell (IntPtr handle, string bindingText) : base(bindingText, handle)
		{       
		}

		public TwoLinesCell (string cellIdentifier, string bindingText, UITableViewCellAccessory accessory) : base( bindingText, UITableViewCellStyle.Subtitle, new NSString(cellIdentifier), accessory  )
		{                   
			SelectionStyle = UITableViewCellSelectionStyle.None;

			Initialize ();
		}

		public string FirstLine 
		{ 
			get { return TextLabel.Text; }
			set { TextLabel.Text = value; }
		}

		public UIColor FirstLineTextColor 
		{ 
			get { return TextLabel.TextColor; } 
			set { TextLabel.TextColor = value; } 
		}

		public string SecondLine 
		{ 
			get { return DetailTextLabel.Text; }
			set { DetailTextLabel.Text = value;}
		}

		public bool ShowPlusSign { 
			get { return _showPlusSign; }
			set { 
				_showPlusSign = value;
				ImageView.Image = value ? UIImage.FromFile("add_location.png") : null;
			}
		}

		public string Icon { 
			get { return _icon; }
			set { 
				_icon = value;
				if ( ShowPlusSign )
				{
					return;
				}

				ImageView.Image = value.HasValue () ? UIImage.FromFile(string.Format ("{0}.png", value)) : null;
			}
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

		public bool ShowRightArrow { 
			get { return _showArrow; }
			set { 
				_showArrow = value;
				_arrowImage.Hidden = !_showArrow;
			}
		}

		private void Initialize ()
		{
			BackgroundView = new CustomCellBackgroundView(  this.ContentView.Frame   ) 
			{
				HideBottomBar = HideBottomBar,              
			};

			TextLabel.TextColor = UIColor.FromRGB(44, 44, 44);
			TextLabel.BackgroundColor = UIColor.Clear;
			TextLabel.Font = UIFont.FromName(FontName.HelveticaNeueBold, 28/2);

			DetailTextLabel.TextColor = UIColor.FromRGB(44, 44, 44);
			DetailTextLabel.BackgroundColor = UIColor.Clear;
			DetailTextLabel.Font = UIFont.FromName(FontName.HelveticaNeueLight, 28/2);

			ContentView.BackgroundColor = UIColor.Clear;
			BackgroundColor = UIColor.Clear;

			if (this.Accessory != UITableViewCellAccessory.None)
			{
				_arrowImage = new UIImageView(new RectangleF(0, 0, 8, 13)); 
				_arrowImage.BackgroundColor = UIColor.Clear;
				_arrowImage.ContentMode = UIViewContentMode.ScaleAspectFit;
				_arrowImage.Image = UIImage.FromFile("right_arrow.png");
				_arrowImage.Hidden = true;

				AccessoryView = _arrowImage;
			}
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{   
			((CustomCellBackgroundView)BackgroundView).Highlighted = true;
			SetNeedsDisplay();
			base.TouchesBegan ( touches, evt ); 
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			((CustomCellBackgroundView) BackgroundView).Highlighted = false;
			SetNeedsDisplay();
			base.TouchesEnded (touches, evt);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			((CustomCellBackgroundView) BackgroundView).Highlighted = false;
			SetNeedsDisplay();
			base.TouchesCancelled (touches, evt);
		}
	}
}

