using System;
using System.Linq;
using Android.Content;
using Android.GoogleMaps;
using Android.Runtime;
using Android.Util;
using Android.Views;
using TaxiMobile.MapUtitilties;

namespace TaxiMobile.Controls
{
    public class TouchMap : MapView
    {

        public event EventHandler MapTouchUp;

        protected TouchMap(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public TouchMap(Context context, string apiKey)
            : base(context, apiKey)
        {
        }

        public TouchMap(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public TouchMap(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
        }


        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                if (MapTouchUp != null)
                {
                    MapTouchUp(this, EventArgs.Empty);
                }
            }

            if (e.Action == MotionEventActions.Move)
            {
                if (this.Overlays != null)
                {
                    foreach (var i in this.Overlays.OfType<PushPinOverlay>())
                    {
                        i.RemoveBaloon();
                    }
                }
            }
            Console.WriteLine(e.Action.ToString());
            return base.DispatchTouchEvent(e);
        }
        public override bool OnTouchEvent(MotionEvent e)
        {
            return base.OnTouchEvent(e);
        }



    }
}
