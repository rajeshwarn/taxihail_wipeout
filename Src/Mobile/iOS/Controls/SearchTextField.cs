using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using System.Drawing;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Register("SearchTextField")]
	public class SearchTextField : TextField
    {

		public SearchTextField(IntPtr handle) : base(handle)
        {
            Initialize();
        }

		public SearchTextField(RectangleF rect) : base( rect )
        {
            Initialize();
        }

        private void Initialize()
        {
			this.EditingChanged += delegate {
				if ( TextChangedCommand != null && TextChangedCommand.CanExecute() )
				{
					TextChangedCommand.Execute( this.Text );
				}			
			};
        }

		public IMvxCommand TextChangedCommand { get; set; }


    }
}