using System;
using System.Threading;
using Android.App;

using apcurium.MK.Common.Diagnostic;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public class ThreadHelper
    {
        private static ProgressDialog _dialog;

        public static void ExecuteInThread(Activity owner, Action action, Action onDismiss, bool showLoading)
        {
            //action();

            if (showLoading)
            {
                _dialog = ProgressDialog.Show(owner, "", owner.GetString(Resource.String.LoadingMessage), true, false);
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                    //throw;
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
            var thread = new Thread(o => { action(); });

            thread.Start();
            return thread;
        }
    }
}