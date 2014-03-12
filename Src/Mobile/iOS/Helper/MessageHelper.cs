using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
	public static class MessageHelper
	{
		
        public static void Show ( string title, string message, string additionalActionTitle , Action additionalAction )
        {
            UIApplication.SharedApplication.InvokeOnMainThread ( delegate
                                                                {                                       
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView ( title, message, null, additionalActionTitle);
                av.Clicked += delegate {
                    additionalAction();     
                };                                      
                av.Show();                                                      
            } );
        }


        public static void Show ( string title, string message, string positiveActionTitle , Action positiveAction, string negativeActionTitle , Action negativeAction, string neutralActionTitle , Action neutralAction )
        {
            
            UIApplication.SharedApplication.InvokeOnMainThread ( delegate
                                                                {                   
                LoadingOverlay.StopAnimatingLoading();
                UIAlertView av;

                if ( neutralAction != null )
                {
                    av = new UIAlertView(title, message, null, Localize.GetValue("Close"), positiveActionTitle, negativeActionTitle, neutralActionTitle);
                }   
                else
                {
                    av = new UIAlertView(title, message, null, Localize.GetValue("Close"), positiveActionTitle, negativeActionTitle);
                }
                av.Clicked += delegate(object sender, UIButtonEventArgs e) {
                    if (e.ButtonIndex == 1) {
                        
                        positiveAction(); 
                    }
                    if (e.ButtonIndex == 2) {
                        
                        negativeAction(); 
                    }
                    if (e.ButtonIndex == 3) {
                        if (neutralAction != null) neutralAction();
                    }
                    };
                av.Show (  );                           
            } );
        }

        public static void Show ( string title, string message, string positiveActionTitle , Action positiveAction, string negativeActionTitle , Action negativeAction)
        {
            
            UIApplication.SharedApplication.InvokeOnMainThread ( delegate
                                                                {                   
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView ( title, message, null,  negativeActionTitle, positiveActionTitle );
                av.Clicked += delegate(object sender, UIButtonEventArgs e) {
                    if (e.ButtonIndex == 0 && negativeAction != null) {
                        negativeAction(); 
                    }
                    else if (e.ButtonIndex == 1 && positiveAction!= null) {
                        positiveAction(); 
                    }                  
                    else
                    {
                        if (positiveAction != null) positiveAction();
                    }
                };
                av.Show (  );                           
            } );
        }

        public static void Show ( string title, string message, List<KeyValuePair<string,Action>> additionalButton)
        {

            UIApplication.SharedApplication.InvokeOnMainThread ( delegate
                                                                {      
                var listTitle = additionalButton.Select(c=>c.Key).ToArray();
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView(title, message, null, Localize.GetValue("Close"), listTitle);
                av.Clicked += delegate(object sender, UIButtonEventArgs e) {
                    if(e.ButtonIndex!=0)
                    {
                        additionalButton[e.ButtonIndex-1].Value();
                    }
                };
                av.Show (  );                           
            } );
        }
		
		
		public static void Show ( string title, string message, Action onDismiss )
		{
			UIApplication.SharedApplication.InvokeOnMainThread ( delegate
			{					
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView(title, message, null, Localize.GetValue("Close"), null);
				av.Dismissed += delegate {
					onDismiss();
				};
				av.Show (  );							
			} );
		}
		
		public static Task Show ( string title, string message )
		{
			var tcs = new TaskCompletionSource<object>();
            UIApplication.SharedApplication.InvokeOnMainThread( delegate
			{					
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView(title, message, null, Localize.GetValue("Close"), null);
				av.Dismissed += delegate {
					tcs.TrySetResult(null);
				};
				av.Show ();
				
			});
			return tcs.Task;
		}
		
		
		public static void Show ( string message )
		{
			UIApplication.SharedApplication.InvokeOnMainThread ( delegate
			{								
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView(Localize.GetValue("GenericTitle"), message, null, Localize.GetValue("Close"), null);
				av.Show (  );
			} );
			
		}	
		
	}
}

