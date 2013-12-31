using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using apcurium.MK.Common.Extensions;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
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

		public SegmentedButton AddButton( string title, string tag )
		{
			var btnWidth = Frame.Width / (_buttons.Count + 1);
			var btn = new SegmentedButton( new RectangleF(_buttons.Count * btnWidth - (_buttons.Count == 0 ? 1 : 0), 0, btnWidth + 1, Frame.Height ), title ) { Tag2 = tag };

			_buttons.ForEach( b => b.Value.Frame = new RectangleF( b.Key * btnWidth - (b.Key == 0 ? 1 : 0), 0, btnWidth + (b.Key == 0 || b.Key == _buttons.Count ? 1 : 0), Frame.Height ) );

			_buttons.Add( _buttons.Count, btn );

			btn.TouchUpInside += HandleTouchUpInside;

			AddSubview( btn );

			return btn;
		}

		void HandleTouchUpInside (object sender, EventArgs e)
		{
			var btn = sender as SegmentedButton;

		    if (btn != null)
		    {
		        btn.Pressed = true;
		        btn.SetNeedsDisplay();
		        _buttons.Values.Where( b => !b.Equals(btn) ).ForEach( bb => {
		                                                                        bb.Pressed = false;
		                                                                        bb.SetNeedsDisplay();
		        });
		    }
		}

		public void SetSelected( int index )
		{
			HandleTouchUpInside( _buttons[ index ], null );
		}
	}
}

