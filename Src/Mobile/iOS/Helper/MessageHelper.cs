using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Controls;
using ObjCRuntime;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
	public static class MessageHelper
	{		
        public static Task Show (string title, string message, string additionalActionTitle, Action additionalAction)
		{
			// to hide keyboard
			UIApplication.SharedApplication.SendAction (Selector.FromHandle(Selector.GetHandle("resignFirstResponder")), null, null, null);

            var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread (delegate {                                       
                LoadingOverlay.StopAnimatingLoading();

                var cav = new CustomAlertView (title, message);

                cav.Dismissed += delegate {
                    additionalAction();
                    tcs.TrySetResult(null);
                };   

                cav.Show();                                                      
            } );

            return tcs.Task;
        }

        public static Task Show (string title, string message)
        {
			// to hide keyboard
			UIApplication.SharedApplication.SendAction (Selector.FromHandle(Selector.GetHandle("resignFirstResponder")), null, null, null);

            var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate {                   
                LoadingOverlay.StopAnimatingLoading();

                var cav = new CustomAlertView(title, message, Localize.GetValue("Close"));
                cav.Dismissed += delegate {
                    tcs.TrySetResult(null);
                }; 
                cav.Show ();
            });

            return tcs.Task;
        }

        public static void Show (string message)
        {
			// to hide keyboard
			UIApplication.SharedApplication.SendAction (Selector.FromHandle(Selector.GetHandle("resignFirstResponder")), null, null, null);

            UIApplication.SharedApplication.InvokeOnMainThread (delegate {                              
                LoadingOverlay.StopAnimatingLoading();

                var cav = new CustomAlertView(Localize.GetValue("GenericTitle"), message, Localize.GetValue("Close"));

                cav.Show ();
            });
        }   

        public static Task Show (string title, string message, string positiveActionTitle , Action positiveAction, string negativeActionTitle , Action negativeAction, string neutralActionTitle , Action neutralAction )
        {
			// to hide keyboard
			UIApplication.SharedApplication.SendAction (Selector.FromHandle(Selector.GetHandle("resignFirstResponder")), null, null, null);

            var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate {                   
                LoadingOverlay.StopAnimatingLoading();

                var cav = new CustomAlertView(title, message, positiveActionTitle, positiveAction, negativeActionTitle, negativeAction, neutralActionTitle, neutralAction);
                cav.Clicked += delegate {
                    tcs.TrySetResult(null);
                }; 
                cav.Dismissed += delegate {
                    tcs.TrySetResult(null);
                }; 
                cav.Show ();
            });

            return tcs.Task;
        }

        public static Task Show (string title, string message, string positiveActionTitle , Action positiveAction, string negativeActionTitle , Action negativeAction)
        {
			// to hide keyboard
			UIApplication.SharedApplication.SendAction (Selector.FromHandle(Selector.GetHandle("resignFirstResponder")), null, null, null);

            var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate {                   
                LoadingOverlay.StopAnimatingLoading();

                var cav = new CustomAlertView(title, message, positiveActionTitle, positiveAction, negativeActionTitle, negativeAction);
                cav.Clicked += delegate {
                    tcs.TrySetResult(null);
                }; 
                cav.Dismissed += delegate {
                    tcs.TrySetResult(null);
                }; 
                cav.Show ();
            });

            return tcs.Task;
        }

        public static Task<string> Prompt (string title, string message, Action cancelAction, bool isNumericOnly, string inputText)
		{
			var tcs = new TaskCompletionSource<string>();

			UIApplication.SharedApplication.InvokeOnMainThread( delegate
			{					
				LoadingOverlay.StopAnimatingLoading();

                    var cav = new CustomAlertView(title, message, cancelAction, isNumericOnly, inputText);

                    cav.Dismissed += delegate {
                        tcs.TrySetCanceled();
                    }; 
                    cav.Clicked += delegate {
                        var res = cav.CustomInputView.Text;
                        tcs.TrySetResult(res);
                    }; 
                    cav.Show ();
			});

			return tcs.Task;
		}
	}
}

