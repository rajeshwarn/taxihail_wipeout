using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Diagnostics;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class TouchGesture :UIGestureRecognizer
    {
        private Stopwatch _stopWatch;

        public event EventHandler TouchBegin;

        public TouchGesture ()
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
            if ( TouchBegin != null )
            {
                TouchBegin( this, EventArgs.Empty );
            }
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


