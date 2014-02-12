using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TouchableMap : SupportMapFragment
    {
        public View mOriginalContentView;

        public TouchableWrapper Surface;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup parent, Bundle savedInstanceState)
        {
            mOriginalContentView = base.OnCreateView(inflater, parent, savedInstanceState);
            Surface = new TouchableWrapper(Activity);
            Surface.AddView(mOriginalContentView);
            return Surface;                          
        }

        public override View View
        {
            get
            {
                return mOriginalContentView;
            }
        }
    }

    public class TouchableWrapper: FrameLayout {

        public event EventHandler<MotionEvent> Touched;

        public TouchableWrapper(Context context) :
        base(context)
        {
        }

        public TouchableWrapper(Context context, IAttributeSet attrs) :
        base(context, attrs)
        {
        }

        public TouchableWrapper(Context context, IAttributeSet attrs, int defStyle) :
        base(context, attrs, defStyle)
        {
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (this.Touched != null)
            {
                this.Touched(this, e);
            }

            return base.DispatchTouchEvent(e);
        }
    }
}