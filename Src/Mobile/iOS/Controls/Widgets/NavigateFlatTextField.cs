using System;
using Foundation;
using CoreGraphics;
using System.Windows.Input;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("NavigateFlatTextField")]
    public class NavigateFlatTextField : FlatTextField
    {
        private UIButton _button;

        public NavigateFlatTextField (IntPtr handle) : base (handle)
        {
            Initialize();
        }

        public NavigateFlatTextField () : base()
        {
            Initialize();
        }

        public NavigateFlatTextField (CGRect frame) : base (frame)
        {
            Initialize();
        }

        private void Initialize ()
        {
            _button = new UIButton(Bounds);
            AddSubview(_button);

            HasRightArrow = true;
        }

        protected UIButton Button
        { 
            get {
                return _button;
            }
        }

        public override void WillMoveToSuperview (UIView newsuper)
        {
            base.WillMoveToSuperview (newsuper);
            Button.TouchUpInside -= HandleTouchUpInside;
            Button.TouchUpInside += HandleTouchUpInside;
        }

        public ICommand NavigateCommand { get; set; }

        void HandleTouchUpInside (object sender, EventArgs e)
        {
            var controller = this.FindViewController();
            if(controller == null) return;

            controller.View.EndEditing(true);
            NavigateCommand.ExecuteIfPossible();
        }
    }
}

