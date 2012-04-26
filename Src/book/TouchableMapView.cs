using System;
using System.Diagnostics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace TaxiMobileApp
{

	public class UserTouchedGesture : UIGestureRecognizer
	{
		
		private Stopwatch _stopWatch;
		
		public UserTouchedGesture ()  : base()
		{
			Initialize();						
		}

					

		public UserTouchedGesture (IntPtr handle) : base(handle)
		{
			Initialize();
		}

		[Export("initWithCoder:")]
		public UserTouchedGesture (NSCoder coder) : base(coder)
		{
			Initialize();
			
		}
		private void Initialize()
		{
			this.DelaysTouchesBegan = false;
			this.DelaysTouchesEnded = false;	
			_stopWatch = new Stopwatch ();
			_stopWatch.Start();
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
			
			var r = _stopWatch.ElapsedMilliseconds;			
			return r;
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

