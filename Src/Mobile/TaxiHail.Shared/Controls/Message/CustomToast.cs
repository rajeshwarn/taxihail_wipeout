using System;
using Android.App;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Animation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public class CustomToast
    {
        private ViewGroup _rootView;
        private RelativeLayout _toastRootView;
        private LinearLayout _toastView;
        private TextView _txtMessage;
        private View _viewToDisplay;

        public CustomToast(Activity owner, string message)
        {
            InitView(owner, message);
        }

        private void InitView(Activity owner, string message)
        {
            _rootView = owner.Window.DecorView.RootView as ViewGroup;

            // We know the view will be attached to the rootview, but we don't want to attach it now
            _viewToDisplay = LayoutInflater.FromContext(owner.ApplicationContext).Inflate(Resource.Layout.CustomToastView, _rootView, false);

            _toastRootView = _viewToDisplay.FindViewById<RelativeLayout>(Resource.Id.CustomToastRootView);
            _toastView = _viewToDisplay.FindViewById<LinearLayout>(Resource.Id.CustomToastView);
            _txtMessage = _viewToDisplay.FindViewById<TextView>(Resource.Id.CustomToastMessage);

            DrawHelper.SupportLoginTextColor(_txtMessage);

            _txtMessage.Text = message;
        }

        public void Show()
        {
            _viewToDisplay.Visibility = ViewStates.Invisible;

            // add the view to the rootview 
            _rootView.AddView(_viewToDisplay);

            _viewToDisplay.SetY(_rootView.Height);
            _viewToDisplay.Visibility = ViewStates.Visible;

            var objectAnimator = ObjectAnimator.OfFloat(_viewToDisplay, "y", _rootView.Height, _rootView.Height-168);
            objectAnimator.SetDuration(300);
            var yAnimator = new AnimatorSet();
            yAnimator.Play(objectAnimator);
            yAnimator.Start(); 
        }

        public void Dismiss()
        {
            var objectAnimator = ObjectAnimator.OfFloat(_viewToDisplay, "y", _rootView.Height-168, _rootView.Height);
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

