using System;
using System.Drawing;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Booking
{
    public class BookLaterDatePicker : MvxView
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

            CancelButton = new FlatButton();
            OrderButton = new FlatButton();
            CancelButton.SetTitle(Localize.GetValue("Cancel"), UIControlState.Normal);
            OrderButton.SetTitle(Localize.GetValue("Order"), UIControlState.Normal);
            FlatButtonStyle.Red.ApplyTo(CancelButton);
            FlatButtonStyle.Green.ApplyTo(OrderButton);

            
        }

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			//This resolve a problem with iOS8 and the sdk 7, before this was executed in the initialize method but was causing a crash on iOS 8. 
			if (DatePicker.Superview == null) {			
				AddSubviews (DatePicker, CancelButton, OrderButton);
			}
		}

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<BookLaterDatePicker, BottomBarViewModel>();

            set.Bind()
                .For(v => v.Command)
                .To(vm => vm.SetPickupDateAndReviewOrder);

            set.Bind(CancelButton)
                .For("TouchUpInside")
                .To(vm => vm.CancelBookLater);
                
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
                Command.Execute(Date);
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

        public ICommand Command { get; set; }

        public DateTime? Date 
        {
            get 
            {
                var d = (DateTime?)DatePicker.Date;
                if (d.HasValue)
                {
                    return d.Value.ToLocalTime();
                }
                return null;
            }
            set 
            {
                if (value == null)
                {
                    DatePicker.SetDate(DateTime.UtcNow, true);
                }
                else 
                {
                    DatePicker.SetDate(value.Value.ToUniversalTime(), true);
                }
            } 
        }
    }
}

