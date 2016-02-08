using System;
using Android.App;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using Android.Animation;
using Android.Content;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Text;
using System.Threading.Tasks;
#if CALLBOX
using apcurium.MK.Callbox.Mobile.Client.Helpers;
using apcurium.MK.Callbox.Mobile.Client;
#endif
using Android.Views.InputMethods;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Dialog
{
    public class CustomAlertDialog
    {
        private ViewGroup _rootView;
        private RelativeLayout _dialogRootView;
        private FrameLayout _dialogOpacityView;
        private LinearLayout _dialogView;
        private Button _buttonCancel;
        private TextView _txtTitle;
        private TextView _txtMessage;
        private Button _twoButtonsPositive;
        private Button _twoButtonsNegative;
        private Button _threeButtonsPositive;
        private Button _threeButtonsNeutral;
        private Button _threeButtonsNegative;
        private LinearLayout _twoButtonsView;
        private LinearLayout _threeButtonsView;
        private EditText _inputText;

        public CustomAlertDialog()
        {
        }

        public CustomAlertDialog(Activity owner, string title, string message, Action onClose = null, string buttonTitle = "")
        {
            InitView(owner, title, message);

            _buttonCancel.Visibility = ViewStates.Visible;

            _buttonCancel.Text = string.IsNullOrEmpty(buttonTitle) ? Mvx.Resolve<ILocalization>()["OkButtonText"] : buttonTitle;
            _buttonCancel.Click += (sender, e) => 
                {  
                    if (onClose != null)
                    {
                        onClose();
                    }
                    HideAnimate();
                };

            showAnimate();
        }

        public CustomAlertDialog(Activity owner, string title, string message, string positiveButtonTitle,
            Action positiveAction, string negativeButtonTitle,
            Action negativeAction)
        {
            InitView(owner, title, message);

            _twoButtonsView.Visibility = ViewStates.Visible;
            _twoButtonsNegative.Text = negativeButtonTitle;
            _twoButtonsNegative.Click += (sender, e) => 
                {  
                    if(negativeAction != null)
                    {
                        negativeAction();
                    }
                    HideAnimate();
                };
            _twoButtonsPositive.Text = positiveButtonTitle;
            _twoButtonsPositive.Click += (sender, e) => 
                {  
                    if(positiveAction != null)
                    {
                        positiveAction();
                    }
                    HideAnimate();
                };

            showAnimate();
        }

        public CustomAlertDialog(Activity owner, string title, string message, string positiveButtonTitle,
            Action positiveAction, string negativeButtonTitle,
            Action negativeAction, string neutralButtonTitle,
            Action neutralAction)
        {
            InitView(owner, title, message);

            _threeButtonsView.Visibility = ViewStates.Visible;
            _threeButtonsNegative.Text = negativeButtonTitle;
            _threeButtonsNegative.Click += (sender, e) => 
                {  
                    if(negativeAction != null)
                    {
                        negativeAction();
                    }
                    HideAnimate();
                };
            _threeButtonsPositive.Text = positiveButtonTitle;
            _threeButtonsPositive.Click += (sender, e) => 
                {  
                    if(positiveAction != null)
                    {
                        positiveAction();
                    }
                    HideAnimate();
                };
            _threeButtonsNeutral.Text = neutralButtonTitle;
            _threeButtonsNeutral.Click += (sender, e) => 
                {  
                    if(neutralAction != null)
                    {
                        neutralAction();
                    }
                    HideAnimate();
                };

            showAnimate();
        }

        private void InitView(Activity owner, string title, string message)
        {
            _rootView = owner.Window.DecorView.RootView as ViewGroup;

            // We know the view will be attached to the rootview, but we don't want to attach it now
            var viewToDisplay = LayoutInflater.FromContext(owner.ApplicationContext).Inflate(Resource.Layout.CustomAlertDialogView, _rootView, false);

            _dialogOpacityView = viewToDisplay.FindViewById<FrameLayout>(Resource.Id.CustomDialogBackView);
            _dialogRootView = viewToDisplay.FindViewById<RelativeLayout>(Resource.Id.CustomDialogRootView);
            _dialogView = viewToDisplay.FindViewById<LinearLayout>(Resource.Id.CustomDialogDialogView);
            _twoButtonsView = viewToDisplay.FindViewById<LinearLayout>(Resource.Id.CustomDialog2ButtonsLayout);
            _threeButtonsView = viewToDisplay.FindViewById<LinearLayout>(Resource.Id.CustomDialog3ButtonsLayout);
            _dialogView = viewToDisplay.FindViewById<LinearLayout>(Resource.Id.CustomDialogDialogView);
            _buttonCancel = viewToDisplay.FindViewById<Button>(Resource.Id.CustomDialogCancelButton);
            _twoButtonsPositive = viewToDisplay.FindViewById<Button>(Resource.Id.CustomDialog2ButtonsPositive);
            _twoButtonsNegative = viewToDisplay.FindViewById<Button>(Resource.Id.CustomDialog2ButtonsNegative);
            _threeButtonsPositive = viewToDisplay.FindViewById<Button>(Resource.Id.CustomDialog3ButtonsPositive);
            _threeButtonsNeutral = viewToDisplay.FindViewById<Button>(Resource.Id.CustomDialog3ButtonsNeutral);
            _threeButtonsNegative = viewToDisplay.FindViewById<Button>(Resource.Id.CustomDialog3ButtonsNegative);
            _txtTitle = viewToDisplay.FindViewById<TextView>(Resource.Id.CustomDialogTitle);
            _txtMessage = viewToDisplay.FindViewById<TextView>(Resource.Id.CustomDialogMessage);
            _inputText = viewToDisplay.FindViewById<EditText>(Resource.Id.CustomDialogInputText);

            DrawHelper.SupportLoginTextColor(_txtTitle);
            DrawHelper.SupportLoginTextColor(_txtMessage);
            DrawHelper.SupportLoginTextColor(_buttonCancel);
            DrawHelper.SupportLoginTextColor(_twoButtonsPositive);
            DrawHelper.SupportLoginTextColor(_twoButtonsNegative);
            DrawHelper.SupportLoginTextColor(_threeButtonsPositive);
            DrawHelper.SupportLoginTextColor(_threeButtonsNeutral);
            DrawHelper.SupportLoginTextColor(_threeButtonsNegative);

            _txtTitle.Text = title;
            _txtTitle.Typeface = Android.Graphics.Typeface.DefaultBold;
            if (message.HasValue())
            {
                _txtMessage.Visibility = ViewStates.Visible;
                _txtMessage.Text = message;
            }

            //Prevent click on ContentView
            _dialogOpacityView.Touch += (sender, e) => {
                e.Handled = true;
                ToogleKeyboard(true, owner);
            };

            // add the view to the rootview 
            _rootView.AddView(viewToDisplay);
        }

        public Task<string> ShowPrompt(Activity owner, string title, string message, Action cancelAction = null, bool isNumericOnly = false, string inputText = "")
        {
            var tcs = new TaskCompletionSource<string> ();

            InitView(owner, title, message);

            var @params = (RelativeLayout.LayoutParams)_dialogView.LayoutParameters;
            @params.RemoveRule(LayoutRules.CenterInParent);
            @params.TopMargin = 200;

            _dialogView.LayoutParameters = @params;

            _inputText.Text = inputText;
            _inputText.InputType = isNumericOnly ? InputTypes.ClassNumber : InputTypes.ClassText;


            _inputText.Visibility = ViewStates.Visible;
            ToogleKeyboard(false, owner);
            _twoButtonsView.Visibility = ViewStates.Visible;
            _twoButtonsNegative.Text = Mvx.Resolve<ILocalization>()["Cancel"];
            _twoButtonsNegative.Click += (sender, e) => 
                {  
                    if(cancelAction != null)
                    {
                        cancelAction();
                    }
                    ToogleKeyboard(true, owner);
                    HideAnimate();
                };
            _twoButtonsPositive.Text = Mvx.Resolve<ILocalization>()["OkButtonText"];
            _twoButtonsPositive.Click += (sender, e) => 
                {  
                    tcs.TrySetResult(_inputText.Text);
                    ToogleKeyboard(true, owner);
                    HideAnimate();
                };

            showAnimate(); 

            return tcs.Task;
        }

        private void ToogleKeyboard(bool hide, Activity owner)
        {
            if (hide)
            {
                var imm = (InputMethodManager)owner.GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(_inputText.WindowToken, 0);
            }
            else
            {
                var imm = (InputMethodManager)owner.GetSystemService(Context.InputMethodService);
                imm.ShowSoftInput(_inputText, ShowFlags.Forced);
                imm.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
                _inputText.RequestFocus();
            }
        }

        private void showAnimate()
        {
            _dialogView.Alpha = 0;
            _dialogOpacityView.Alpha = 0;
            var objectAnimatorDialog = ObjectAnimator.OfFloat(_dialogView, "alpha", 0, 1);
            objectAnimatorDialog.SetDuration(300);
            var objectAnimatorBackView = ObjectAnimator.OfFloat(_dialogOpacityView, "alpha", 0, 0.45f);
            objectAnimatorBackView.SetDuration(300);
            var opacityAnimator = new AnimatorSet();
            opacityAnimator.PlayTogether(objectAnimatorDialog, objectAnimatorBackView);
            opacityAnimator.Start(); 
        }

        public void HideAnimate()
        {
            var objectAnimator = ObjectAnimator.OfFloat(_dialogView, "alpha", 1, 0);
            objectAnimator.SetDuration(300);
            var objectAnimatorBackView = ObjectAnimator.OfFloat(_dialogOpacityView, "alpha", 0.45f, 0);
            objectAnimatorBackView.SetDuration(300);
            var opacityAnimator = new AnimatorSet();
            opacityAnimator.PlayTogether(objectAnimator, objectAnimatorBackView);
            opacityAnimator.AnimationEnd += (sender, e) => 
                {
                    _rootView.RemoveView(_dialogRootView);
                };
            opacityAnimator.Start();

            AlertDialogHelper.LatestAlert = null;
        }
    }
}

