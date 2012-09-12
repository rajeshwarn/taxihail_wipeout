using System;
using System.Collections.Generic;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Common.Extensions;
using System.Linq;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("SegmentedButtonBar")]
	public class SegmentedButtonBar : UIView
	{
		private Dictionary<int, SegmentedButton> _buttons; 

        public SegmentedButtonBar(IntPtr handle) : base(  handle )
        {
			Initialize();
        }

		public SegmentedButtonBar ( RectangleF rect ) : base( rect )
		{
			 Initialize();
		}

		private void Initialize()
		{
			_buttons = new Dictionary<int, SegmentedButton>();
			BackgroundColor = UIColor.Clear;
		}

		public void AddButton( string title, Action onClick )
		{
			var btnWidth = this.Frame.Width / (_buttons.Count + 1);
			var btn = new SegmentedButton( new RectangleF(_buttons.Count * btnWidth - (_buttons.Count == 0 ? 1 : 0), 0, btnWidth + 1, Frame.Height ), title, onClick );

			_buttons.ForEach( b => b.Value.Frame = new RectangleF( b.Key * btnWidth - (b.Key == 0 ? 1 : 0), 0, btnWidth + (b.Key == 0 || b.Key == _buttons.Count ? 1 : 0), Frame.Height ) );

			_buttons.Add( _buttons.Count, btn );

			btn.TouchUpInside += HandleTouchUpInside;

			AddSubview( btn );
		}

		void HandleTouchUpInside (object sender, EventArgs e)
		{
			var btn = sender as SegmentedButton;

			if( btn.OnClick != null )
			{
				btn.OnClick();
			}

			btn.Pressed = true;
			btn.SetNeedsDisplay();
			_buttons.Values.Where( b => !b.Equals(btn) ).ForEach( bb => {
				bb.Pressed = false;
				bb.SetNeedsDisplay();
			});
		}

		public void SetSelected( int index )
		{
			HandleTouchUpInside( _buttons[ index ], null );
		}
	}
}

