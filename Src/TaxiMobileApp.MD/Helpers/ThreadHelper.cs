using System;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using Android.App;
using TaxiMobile;
using TaxiMobileApp;



namespace TaxiMobile.Helpers
{
    public class ThreadHelper
    {
        public ThreadHelper()
        {
        }

        private static ProgressDialog _dialog;
        public static void ExecuteInThread(Activity owner, Action action, bool showLoading)
        {
            ExecuteInThread(owner, action, null, showLoading);
        }
        public static void ExecuteInThread(Activity owner, Action action, Action onDismiss, bool showLoading)
		{
			//action();

            if (showLoading)
            {
                _dialog = ProgressDialog.Show(owner, "", owner.GetString(Resource.String.LoadingMessage), true, false);
            }

			ThreadPool.QueueUserWorkItem (o =>
			{


                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);
                    throw;
                }
                finally
                {
                    if (owner != null)
                    {

                        owner.RunOnUiThread(() =>
                                                {
                                                    if (_dialog != null)
                                                    {
                                                        _dialog.Cancel();
                                                        _dialog.Dispose();
                                                        _dialog = null;
                                                    }
                                                    if (onDismiss != null)
                                                    {
                                                        onDismiss();
                                                    }
                                                });
                    }
                }
				
			});
		}

        public static Thread ExecuteInThreadWithPool(Action action)
        {


            var thread = new Thread(o =>
            {

                action();

            });

            thread.Start();
            return thread;
        }


    }
}

