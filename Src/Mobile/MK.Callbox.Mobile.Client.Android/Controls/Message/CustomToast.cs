using System;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Animation;
using Android.Util;
using Java.Interop;
using Android.Content;
using Cirrious.CrossCore;
using apcurium.MK.Common;
using apcurium.MK.Callbox.Mobile.Client.Helpers;
using apcurium.MK.Callbox.Mobile.Client;


namespace apcurium.MK.Callbox.Mobile.Client.Controls.Message
{
    public class CustomToast
    {
        private ViewGroup _rootView;
        private TextView _txtMessage;
        private LinearLayout _btnDismiss;
        private View _viewToDisplay;
        private Activity _owner;

        public CustomToast(Activity owner, string message)
        {
            _owner = owner;
            InitView(message);
        }

        private void InitView(string message)
        {
            _rootView = _owner.Window.DecorView.RootView as ViewGroup;

            // We know the view will be attached to the rootview, but we don't want to attach it now
            _viewToDisplay = LayoutInflater.FromContext(_owner.ApplicationContext).Inflate(Resource.Layout.CustomToastView, _rootView, false);

            _txtMessage = _viewToDisplay.FindViewById<TextView>(Resource.Id.CustomToastMessage);
            _btnDismiss = _viewToDisplay.FindViewById<LinearLayout>(Resource.Id.CustomToastButton);

            DrawHelper.SupportLoginTextColor(_txtMessage);
            _txtMessage.Text = message;

            _btnDismiss.Click += (object sender, EventArgs e) => 
                {
                    Dismiss();
                    Mvx.Resolve<IConnectivityService>().ToastDismissed();
                };
        }

        public bool Show()
        {
            // Prevent multiple toasts to be displayed
            if (_rootView.ChildCount > 1)
            {
                return false;
            }

            // Hide toast while setting its place on screen
            _viewToDisplay.Visibility = ViewStates.Invisible;

            // add the view to the rootview 
            _rootView.AddView(_viewToDisplay);

            // Get size of the screen because sometimes _rootView.Height == 0
            var display = _owner.ApplicationContext.GetSystemService(Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay;
            var metrics = new DisplayMetrics();
            display.GetMetrics(metrics);

            // Set the Toast a the bottom of the view
            _viewToDisplay.SetY(metrics.HeightPixels);

            // Now we can make it visible
            _viewToDisplay.Visibility = ViewStates.Visible;

            // Slide in animation
            var objectAnimator = ObjectAnimator.OfFloat(_viewToDisplay, "y", metrics.HeightPixels, metrics.HeightPixels-_viewToDisplay.LayoutParameters.Height);
            objectAnimator.SetDuration(300);
            var yAnimator = new AnimatorSet();
            yAnimator.Play(objectAnimator);
            yAnimator.Start(); 

            return true;
        }

        public void Dismiss()
        {
            // Slide out animation
            var objectAnimator = ObjectAnimator.OfFloat(_viewToDisplay, "y", _rootView.Height-_viewToDisplay.LayoutParameters.Height, _rootView.Height);
            objectAnimator.SetDuration(300);
            var yAnimator = new AnimatorSet();
            yAnimator.Play(objectAnimator);
            yAnimator.AnimationEnd += (sender, e) => 
                {
                    _rootView.RemoveView(_viewToDisplay);
                };
            yAnimator.Start();
        }
    }
}

