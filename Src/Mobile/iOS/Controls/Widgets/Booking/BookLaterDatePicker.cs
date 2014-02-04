using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Localization;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Booking
{
    public class BookLaterDatePicker : BaseBindableView<BookLaterViewModel>
    {
        FlatButton CancelButton { get; set; }
        FlatButton OrderButton { get; set; }
        UIDatePicker DatePicker { get; set; }

        public BookLaterDatePicker (IntPtr ptr):base(ptr)
        {
            Initialize ();    
            this.DelayBind (() => {
                InitializeBinding();
            });
        }

        public BookLaterDatePicker ()
        {
            Initialize ();
            this.DelayBind (() => {
                InitializeBinding();
            });
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.White;

            DatePicker = new UIDatePicker();
            DatePicker.ValueChanged += (sender, e) => {
                _date = DatePicker.Date;
            };

            CancelButton = new FlatButton();
            OrderButton = new FlatButton();
            CancelButton.SetTitle(Localize.GetValue("Cancel"), UIControlState.Normal);
            OrderButton.SetTitle(Localize.GetValue("Order"), UIControlState.Normal);
            FlatButtonStyle.Red.ApplyTo(CancelButton);
            FlatButtonStyle.Green.ApplyTo(OrderButton);

            CancelButton.TouchUpInside += (sender, e) => {
                Hide();
            };

            AddSubviews(DatePicker, CancelButton, OrderButton);
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<BookLaterDatePicker, BookLaterViewModel>();

            set.Bind()
                .For(v => v.BookTaxiCommand)
                .To(vm => vm.SetPickupDateAndBook);

            set.Apply();
        }

        float _bottom = 0;
        public void UpdateView(float bottom, float width)
        {
            var buttonHorizontalPadding = 8f;
            var buttonVerticalPadding = 5f;
            var buttonWidth = 97f;
            _bottom = bottom;

            CancelButton.Frame = new RectangleF(buttonHorizontalPadding, buttonVerticalPadding, buttonWidth, 36f);
            OrderButton.Frame = new RectangleF(width - buttonWidth - buttonHorizontalPadding, buttonVerticalPadding, buttonWidth, 36f);

            OrderButton.TouchUpInside += (sender, e) => {
                BookTaxiCommand.Execute(_date);
            };

            DatePicker.SetY(OrderButton.Frame.Bottom + buttonVerticalPadding);

            this.SetWidth(width)
                .SetHeight(DatePicker.Frame.Bottom)
                .SetY(bottom - Frame.Height);
        }

        public void Hide()
        {
            BeginAnimations("SlideDown");
            this.SetY(_bottom);
            CommitAnimations();
            Date = null;
        }

        public void Show()
        {
            _bottom = this.Frame.Y;
            BeginAnimations("SlideUp");
            this.SetY(_bottom - DatePicker.Frame.Bottom);
            CommitAnimations();
        }

        public ICommand BookTaxiCommand { get; set; }

        public DateTime? _date;
        public DateTime? Date 
        {
            get 
            {
                return _date;
            }
            set 
            {
                if(value == null)
                {
                    DatePicker.SetDate(DateTime.Now, true);
                    _date = null;
                }
                else
                {
                    DatePicker.SetDate(value, true);
                }    
            } 
        }
    }
}

