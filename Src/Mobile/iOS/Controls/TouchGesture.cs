using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;

using System.Diagnostics;

using apcurium.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class TouchGesture :UIGestureRecognizer
    {

         private Stopwatch _stopWatch;
        private TouchGesture _gesture;
        public TouchGesture ()  : base()
        {
            Initialize();                       
        }

                    

        public TouchGesture (IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public TouchGesture (NSCoder coder) : base(coder)
        {
            Initialize();
            
        }
        private void Initialize()
        {
            this.DelaysTouchesBegan = false;
            this.DelaysTouchesEnded = false;    
            _stopWatch = new Stopwatch ();

        }
        
        public override bool CanBePreventedByGestureRecognizer (UIGestureRecognizer preventingGestureRecognizer)
        {
            return false;
        }
        
        public bool UserIsTouching {
            get;
            set;
        }
        
        public long GetLastTouchDelay()
        {
            if ( _stopWatch.IsRunning )
            {
            var r = _stopWatch.ElapsedMilliseconds;         
            return r;
            }
            else
            {
                return long.MaxValue;
            }
        }
        
        
        public override void TouchesBegan (NSSet touches, UIEvent evt)
        {                       
            UserIsTouching = true;
            base.TouchesBegan (touches, evt);
        }
        
        public override void TouchesCancelled (NSSet touches, UIEvent evt)
        {
            UserIsTouching = false;
            _stopWatch.Start();
            base.TouchesCancelled (touches, evt);
        }
        
        public override void TouchesEnded (NSSet touches, UIEvent evt)
        {           
                
            UserIsTouching = false;
            _stopWatch.Reset();
            _stopWatch.Start();
            
            base.TouchesEnded (touches, evt);
        
        }
        
    }
}


