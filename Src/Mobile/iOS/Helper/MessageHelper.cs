using System;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class MessageHelper
	{
		public MessageHelper ()
		{
		}
		
		public static void Show ( string title, string message, string additionalActionTitle , Action additionalAction )
		{

			UIApplication.SharedApplication.InvokeOnMainThread ( delegate
			{					
                LoadingOverlay.StopAnimatingLoading();
				var av = new UIAlertView ( title, message, null, Resources.Close, additionalActionTitle );
				av.Clicked += delegate(object sender, UIButtonEventArgs e) {
				if (e.ButtonIndex == 1) {
				
						additionalAction();						
				}};
					
				av.Show (  );							
			} );
		}
		
		
		public static void Show ( string title, string message, Action onDismiss )
		{
			UIApplication.SharedApplication.InvokeOnMainThread ( delegate
			{					
                LoadingOverlay.StopAnimatingLoading();
				var av = new UIAlertView ( title, message, null, Resources.Close, null );
				av.Dismissed += delegate {
					onDismiss();
				};
				av.Show (  );							
			} );
		}
		
		public static void Show ( string title, string message )
		{

            UIApplication.SharedApplication.InvokeOnMainThread( delegate
			{					
                LoadingOverlay.StopAnimatingLoading();
				var av = new UIAlertView ( title, message, null, Resources.Close, null );
				av.Show (  );
				
			} );
		}
		
		
		public static void Show ( string message )
		{
			UIApplication.SharedApplication.InvokeOnMainThread ( delegate
			{								
                LoadingOverlay.StopAnimatingLoading();
				var av = new UIAlertView (  Resources.GenericTitle, message, null, Resources.Close, null );
				av.Show (  );
			} );
			
		}
	
		public static void ShowToast ( string message, int duration )
		{
			
			UIApplication.SharedApplication.InvokeOnMainThread ( delegate 
			                                                  {									
                LoadingOverlay.StopAnimatingLoading();
                var toast = new ToastMessage( AppContext.Current.Controller.TopViewController.View, message );
				toast.Show(duration);
			} );
			
		}
	}
}

