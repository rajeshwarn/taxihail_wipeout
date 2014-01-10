using System;
using apcurium.MK.Booking.Mobile.Client.Localization;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreAnimation;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	[Register("PaymentSelector")]
	public class PaymentSelector : UIView
	{
		public event EventHandler ValueChanged;

		private static readonly UIColor SelectedBackground = UIColor.FromRGB(115, 117, 112);
		private static readonly UIColor NotSelectedBackground = UIColor.White;
		private static readonly UIColor NotSelectedFont = UIColor.FromRGB(115, 117, 112);
		private static readonly UIColor SelectedFont = UIColor.White;

		UIButton _creditCardButton;
		UIButton _payPalButton;

		private bool _payPalSelected;
		public bool PayPalSelected {
			get {
				return _payPalSelected;
			}
			set {
				_payPalSelected = value;

				_payPalButton.Selected = value;
				_creditCardButton.Selected = !value;

				_payPalButton.BackgroundColor = value ? SelectedBackground : NotSelectedBackground;
				_creditCardButton.BackgroundColor = !value ? SelectedBackground : NotSelectedBackground;

				if (ValueChanged != null) {
					ValueChanged.Invoke (this, new EventArgs ());
				}
			}
		}

		public PaymentSelector(RectangleF rect) : base(rect)
		{
			Initialize ();
		}

		public PaymentSelector(IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		void Initialize ()
		{
			var borderRadius = 5f;
			var font = AppStyle.GetNormalFont (14f);

			BackgroundColor = UIColor.Clear;
			this.RoundCorners(borderRadius, 1.5f, UIColor.FromRGB(115, 117, 112));

			_creditCardButton = new UIButton(new RectangleF (Bounds.X, Bounds.Y, Bounds.Width / 2, Bounds.Height));
			_creditCardButton.ClipsToBounds = true;
			_creditCardButton.SetTitle(Localize.GetValue("CreditCard"), UIControlState.Normal);
			_creditCardButton.Font = font;
			_creditCardButton.TouchUpInside += (s,e) => { PayPalSelected = false; };
			_creditCardButton.SetTitleColor(NotSelectedFont, UIControlState.Normal);
			_creditCardButton.SetTitleColor(SelectedFont, UIControlState.Selected);

			_payPalButton = new UIButton (new RectangleF (Bounds.Width / 2, Bounds.Y, Bounds.Width / 2, Bounds.Height));
			_payPalButton.ClipsToBounds = true;
			_payPalButton.SetTitle(Localize.GetValue("View_PayPal"), UIControlState.Normal);
			_payPalButton.Font = font;
			_payPalButton.TouchUpInside += (s,e) => { PayPalSelected = true; };
			_payPalButton.SetTitleColor(NotSelectedFont, UIControlState.Normal);
			_payPalButton.SetTitleColor(SelectedFont, UIControlState.Selected);

			var creditCardMaskPath = UIBezierPath.FromRoundedRect (_creditCardButton.Bounds, UIRectCorner.TopLeft | UIRectCorner.BottomLeft, new SizeF (borderRadius, borderRadius));
			var creditCardMaskLayer = new CAShapeLayer ();
			creditCardMaskLayer.Frame = _creditCardButton.Bounds;
			creditCardMaskLayer.Path = creditCardMaskPath.CGPath;
			_creditCardButton.Layer.Mask = creditCardMaskLayer;

			var payPalMaskPath = UIBezierPath.FromRoundedRect (_payPalButton.Bounds, UIRectCorner.TopRight | UIRectCorner.BottomRight, new SizeF (borderRadius, borderRadius));
			var payPalMaskLayer = new CAShapeLayer ();
			payPalMaskLayer.Frame = _payPalButton.Bounds;
			payPalMaskLayer.Path = payPalMaskPath.CGPath;
			_payPalButton.Layer.Mask = payPalMaskLayer;

			AddSubview (_creditCardButton);
			AddSubview (_payPalButton);
		}
	}
}

