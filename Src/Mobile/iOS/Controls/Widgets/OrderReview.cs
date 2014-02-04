using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Collections;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class OrderReview : MvxView
    {
        public OrderReview(IntPtr handle) : base(handle)
        {

        }

        private IEnumerable _items;
        public IEnumerable Items
        {
            get { return _items; }
            set
            {
                if(_items != value)
                {
                    _items = value;
                    OnItemsChanged();
                }
            }
        }

        private void OnItemsChanged()
        {
            foreach (var subview in Subviews)
            {
                subview.RemoveFromSuperview();
            }

        }
    }
}

