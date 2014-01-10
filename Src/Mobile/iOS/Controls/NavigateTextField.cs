using System;
using System.Drawing;
using System.Windows.Input;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("NavigateTextField")]
    public class NavigateTextField: TextFieldWithArrow
    {
        public NavigateTextField(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        public NavigateTextField(RectangleF rect) : base( rect )
        {
            Initialize();
        }
        
        private void Initialize() {
        }

        public override void WillMoveToSuperview (UIView newsuper)
        {
            base.WillMoveToSuperview (newsuper);
            Button.TouchUpInside -= HandleTouchUpInside;
            Button.TouchUpInside += HandleTouchUpInside;
        }

		public ICommand NavigateCommand {
// ReSharper disable once UnusedAutoPropertyAccessor.Global
            get;set;
        }

        void HandleTouchUpInside (object sender, EventArgs e)
        {
            var controller = this.FindViewController();
            if(controller == null) return;

            controller.View.EndEditing(true);
			if(NavigateCommand != null) NavigateCommand.Execute();

        }
    }
}

