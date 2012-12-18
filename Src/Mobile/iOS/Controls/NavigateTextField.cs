using System;
using System.Drawing;
using MonoTouch.Foundation;
using Cirrious.MvvmCross.Interfaces.Commands;

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

        public override void WillMoveToSuperview (MonoTouch.UIKit.UIView newsuper)
        {
            base.WillMoveToSuperview (newsuper);
            base.Button.TouchUpInside -= HandleTouchUpInside;
            base.Button.TouchUpInside += HandleTouchUpInside;

        }

        public IMvxCommand NavigateCommand {
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

