using System;
using UIKit;
using System.Linq;
using CoreGraphics;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("HideableView")]
	public class HideableView:UIView
	{
		private NSLayoutConstraint[] _hiddenContraints { get; set; }

		public HideableView(IntPtr handle) : base(handle)
		{
		}

		public HideableView(CGRect frame) : base(frame)
		{
		}

		public HideableView() : base()
		{
		}

		public bool HiddenWithConstraints
		{
			get
			{
				return base.Hidden;
			}
			set
			{
				if (base.Hidden != value && value)
				{
					base.Hidden = value;

					_hiddenContraints = this.Superview.Constraints != null 
						? this.Superview.Constraints.Where(x => x.FirstItem == this || x.SecondItem == this).ToArray()
						: null;

					this.Superview.RemoveConstraints(_hiddenContraints);
				}

				if (!value && _hiddenContraints != null)
				{
					base.Hidden = value;
					this.Superview.AddConstraints(_hiddenContraints);
					_hiddenContraints = null;
				}
			}
		}
	}
}