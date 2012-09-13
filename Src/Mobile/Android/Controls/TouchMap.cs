using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.GoogleMaps;
using Android.Runtime;
using Android.Util;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TouchMap : MapView
    {

        public event EventHandler MapTouchUp;

        private Address _pickup;
        private Address _dropoff;

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


        public override bool DispatchTouchEvent(Android.Views.MotionEvent e)
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

        private PushPinOverlay _pickupPin;
        private PushPinOverlay _dropoffPin;

        public Address Pickup
        {
            get { return _pickup; }
            set 
            { 
                _pickup = value;
                if (_pickupPin != null)
                {
                    this.Overlays.Remove(_pickupPin);
                    _pickupPin = null;
                }

                if ((value != null) && (value.Latitude != 0) && (value.Longitude != 0))
                {
                    _pickupPin = MapUtitilties.MapService.AddPushPin(this, Resources.GetDrawable(Resource.Drawable.pin_green), value, Resources.GetString(Resource.String.PickupMapTitle));
                }
            }
        }



        public Address Dropoff
        {
            get { return _dropoff; }
            set 
            { 
               
                 
               
                _dropoff = value;
                if (_dropoffPin != null)
                {
                    this.Overlays.Remove(_dropoffPin);
                    _dropoffPin = null;
                }
                if ((value != null) && (value.Latitude != 0) && (value.Longitude != 0))
                {
                    _dropoffPin = MapUtitilties.MapService.AddPushPin(this, Resources.GetDrawable(Resource.Drawable.pin_red), value, Resources.GetString(Resource.String.DestinationMapTitle));
                }               
            }
        }


        public override bool OnTouchEvent(Android.Views.MotionEvent e)
        {
            return base.OnTouchEvent(e);
        }



    }
}
