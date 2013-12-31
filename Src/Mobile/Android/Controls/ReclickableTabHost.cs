using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class ReclickableTabHost : TabHost
    {
        public delegate void OnTabChangedHandler(int tab);

        public ReclickableTabHost(Context context) : base(context)
        {
        }

        public ReclickableTabHost(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public ReclickableTabHost(IntPtr ptr, JniHandleOwnership handle) : base(ptr, handle)
        {
        }

        public override int CurrentTab
        {
            get { return base.CurrentTab; }
            set
            {
                base.CurrentTab = value;
                if (OnTabChanged != null)
                {
                    OnTabChanged(value);
                }
            }
        }

        public event OnTabChangedHandler OnTabChanged;
    }
}