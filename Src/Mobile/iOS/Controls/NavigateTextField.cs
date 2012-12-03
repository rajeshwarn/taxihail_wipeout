using System;
using System.Drawing;
using MonoTouch.Foundation;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client
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
            EditingDidBegin += HandleEditingDidBegin;
        }

        public IMvxCommand NavigateCommand {
            get;set;
        }

        void HandleEditingDidBegin (object sender, EventArgs e)
        {
            var controller = this.FindViewController();
            if(controller == null) return;

            controller.View.EndEditing(true);
            if(NavigateCommand != null) NavigateCommand.Execute();

        }
    }
}

