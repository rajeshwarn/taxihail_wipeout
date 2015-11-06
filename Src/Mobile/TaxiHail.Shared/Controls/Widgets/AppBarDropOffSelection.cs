using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Runtime;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarDropOffSelection")]
    public class AppBarDropOffSelection: MvxFrameControl
    {
        public AppBarDropOffSelection(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_AppBar_DropOffSelection,context, attrs)
        {
        }
    }
}