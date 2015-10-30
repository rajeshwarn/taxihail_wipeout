using System;
using UIKit;
using System.Linq;
using CoreGraphics;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("HindableView")]
	public class HindableView:UIView
	{
		private NSLayoutConstraint[] _hiddenContraints { get; set; }

		public HindableView(IntPtr handle) : base(handle)
		{
		}

		public HindableView(CGRect frame) : base(frame)
		{
		}

		public HindableView() : base()
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
				if (base.Hidden != value)
				{
					base.Hidden = value;
					if (value)
					{
						_hiddenContraints = this.Superview.Constraints != null 
							? this.Superview.Constraints.Where(x => x.FirstItem == this || x.SecondItem == this).ToArray()
							: null;
						if (_hiddenContraints != null)
						{
							this.Superview.RemoveConstraints(_hiddenContraints);
						}
					}
					else
					{
						if (_hiddenContraints != null)
						{
							this.Superview.AddConstraints(_hiddenContraints);
							_hiddenContraints = null;
						}
					}
				}
			}
		}
	}
}