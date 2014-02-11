using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderEdit : MvxLinearLayout
    {
        public OrderEdit(Context context, IAttributeSet attrs)
            :base(context, attrs)
        {

        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.SubView_OrderEdit, this, true);
        }
    }
}

