using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public abstract class BaseBindableChildView<TViewModel> : OverlayView
        where TViewModel : ChildViewModel
    {
        public BaseBindableChildView(IntPtr handle) : base(handle)
        {
        }

        public new TViewModel ViewModel
        {
            get
            {
                return (TViewModel)DataContext;
            }
        }

        protected void DismissKeyboardOnReturn (params UITextField[] textFields)
        {
            if (textFields == null)
                return;

            foreach (var textField in textFields) 
            {
                textField.ReturnKeyType = UIReturnKeyType.Done;
                textField.ShouldReturn = ShouldReturn;
            }
        }

        private bool ShouldReturn (UITextField textField)
        {
            return textField.ResignFirstResponder();
        }
    }
}

