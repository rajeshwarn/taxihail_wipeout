using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
	public static class MessageHelper
	{
		
        public static Task Show (string title, string message, string additionalActionTitle, Action additionalAction)
        {
            var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread (delegate {                                       
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView (title, message, null, additionalActionTitle);
                av.Clicked += delegate {
                    additionalAction();
                    tcs.TrySetResult(null);
                };                                      
                av.Show();                                                      
            } );

            return tcs.Task;
        }

        public static Task Show (string title, string message, string positiveActionTitle , Action positiveAction, string negativeActionTitle , Action negativeAction, string neutralActionTitle , Action neutralAction )
        {
            var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread (delegate {                   
                LoadingOverlay.StopAnimatingLoading();
                UIAlertView av;

                if (neutralAction != null)
                {
                    av = new UIAlertView(title, message, null, null, positiveActionTitle, negativeActionTitle, neutralActionTitle);
                }   
                else
                {
                    av = new UIAlertView(title, message, null, Localize.GetValue("Close"), positiveActionTitle, negativeActionTitle);
                }

                av.Clicked += delegate(object sender, UIButtonEventArgs e) {
                    if (e.ButtonIndex == 0) 
                    {
                        positiveAction(); 
                    }
                    if (e.ButtonIndex == 1) 
                    {
                        negativeAction(); 
                    }
                    if (e.ButtonIndex == 2) 
                    {
                        if (neutralAction != null) 
                        {
                            neutralAction();
                        }
                    }
                    tcs.TrySetResult(null);
                };

                av.Show ();                           
            });

            return tcs.Task;
        }

        public static Task Show (string title, string message, string positiveActionTitle , Action positiveAction, string negativeActionTitle , Action negativeAction)
        {
            return Show(title, message, positiveActionTitle, positiveAction, negativeActionTitle, negativeAction, () => { });
        }

        public static Task Show(string title, string message, string positiveActionTitle, Action positiveAction, string negativeActionTitle, Action negativeAction, Action cancelAction)
        {
            var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView(title, message, null, negativeActionTitle, positiveActionTitle);
                av.Canceled += (sender, args) => cancelAction();
                av.Clicked += delegate(object sender, UIButtonEventArgs e)
                {
                    if (e.ButtonIndex == 0 && negativeAction != null)
                    {
                        negativeAction();
                    }
                    else if (e.ButtonIndex == 1 && positiveAction != null)
                    {
                        positiveAction();
                    }
                    else
                    {
                        if (positiveAction != null)
                        {
                            positiveAction();
                        }
                    }
                    tcs.TrySetResult(null);
                };
                av.Show();
            });

            return tcs.Task;
        }

        public static Task Show (string title, string message, List<KeyValuePair<string,Action>> additionalButton)
        {
            var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread (delegate {      
                var listTitle = additionalButton.Select(c => c.Key).ToArray();
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView(title, message, null, Localize.GetValue("Close"), listTitle);
                av.Clicked += delegate(object sender, UIButtonEventArgs e) {
                    if(e.ButtonIndex != 0)
                    {
                        additionalButton[(int)e.ButtonIndex - 1].Value();
                    }
                    tcs.TrySetResult(null);
                };
                av.Show ();                           
            });

            return tcs.Task;
        }

		public static Task Show (string title, string message)
		{
			var tcs = new TaskCompletionSource<object>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate {					
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView(title, message, null, Localize.GetValue("Close"), null);
				av.Dismissed += delegate {
					tcs.TrySetResult(null);
				};
				av.Show ();
			});

			return tcs.Task;
		}
		
		public static void Show (string message)
		{
			UIApplication.SharedApplication.InvokeOnMainThread (delegate {								
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView(Localize.GetValue("GenericTitle"), message, null, Localize.GetValue("Close"), null);
				av.Show ();
			});
		}	

		public static Task<string> Prompt (string title, string message, Action cancelAction, bool isNumericOnly = false)
		{
			var tcs = new TaskCompletionSource<string>();

			UIApplication.SharedApplication.InvokeOnMainThread( delegate
			{					
				LoadingOverlay.StopAnimatingLoading();
				var av = new UIAlertView(title, message, null, Localize.GetValue("Cancel"), Localize.GetValue("OkButtonText"));
				av.AlertViewStyle = UIAlertViewStyle.PlainTextInput;

				if(isNumericOnly)
				{
					var textField = av.GetTextField(0);
					textField.KeyboardType = UIKeyboardType.NumberPad;
				}
				av.Dismissed += (sender, e) => 
				{
					if(e.ButtonIndex == 0)
					{
						tcs.TrySetCanceled();
						cancelAction();
					}
					else
					{
						var value = ((UIAlertView)sender).GetTextField(0).Text;
						tcs.TrySetResult(value);
					}
				};
				av.Show ();
			});

			return tcs.Task;
		}
	}
}

