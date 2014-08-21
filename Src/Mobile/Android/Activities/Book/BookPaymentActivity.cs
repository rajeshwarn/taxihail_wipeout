using System.Windows.Input;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "BookPaymentActivity", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookPaymentActivity : BaseBindingActivity<PaymentViewModel>
    {
        private EditText _meterAmountTextView;
        private EditText _tipAmountTextView;
        private EditTextRightSpinner _tipPicker;
        private TextView _totalAmountTextView;

        protected override void OnStart()
        {
            base.OnStart();
			            
            _tipPicker = FindViewById<EditTextRightSpinner>(Resource.Id.tipPicker);
            _tipAmountTextView = FindViewById<EditText>(Resource.Id.tipAmountTextView);
            _meterAmountTextView = FindViewById<EditText>(Resource.Id.meterAmountTextView);
            _totalAmountTextView = FindViewById<TextView>(Resource.Id.totalAmountTextView);

            // No live binding with Text in MvvmCross on Android
            // Asked on SO: http://stackoverflow.com/questions/21461759/setter-on-text-only-called-after-edit-on-android-live-on-ios
            // Live binding for meter amount
            _meterAmountTextView.AfterTextChanged += (sender, e) =>
            {
                var value = ViewModel.MeterAmount ;
                var newValue = e.Editable.ToString();
                if (value!=newValue)
                {
                    ViewModel.MeterAmount = newValue;               
                }
            };

            // Live binding for tip amount
            _tipAmountTextView.AfterTextChanged += (sender, e) =>
            {
                var value = ViewModel.TipAmount ;
                var newValue = e.Editable.ToString();
                if (value!=newValue)
                {
                    ViewModel.TipAmount = newValue;               
                }
            };

            // Should be an ImeActions.Next binding on MeterAmount EditText:
            _meterAmountTextView.EditorAction += (object sender, TextView.EditorActionEventArgs e) =>  
            {
                if (e.ActionId == Android.Views.InputMethods.ImeAction.Done)
                {
                    ViewModel.ShowCurrencyCommand.Execute();
                    _meterAmountTextView.HideKeyboard();
                }
            };

            // Should be an ImeActions.Done binding on TipAmount EditText:
            _tipAmountTextView.EditorAction += (object sender, TextView.EditorActionEventArgs e) =>  
            {
                if (e.ActionId == Android.Views.InputMethods.ImeAction.Done)
                {
                    ViewModel.ShowCurrencyCommand.Execute();
                    _tipAmountTextView.HideKeyboard();
                }
            };


            // Added to overcome a bug: can't notify anything when re-selecting same value when coming back from custom tip, so created a general 
            // Should eventually become a binding
            _tipPicker.SpinnerClicked += (object sender, System.EventArgs e) =>
            {
                if (ViewModel.PaymentPreferences.TipListDisabled)
                {
                    // Clear should be re-written to considere listDisabled
                    ViewModel.ClearTipCommand.Execute();
                }
            };
            
            // Should be an End Edit binding on MeterAmount EditText:
            _meterAmountTextView.FocusChange += (sender, e) =>
            {
                if (!e.HasFocus)
                {
                    ViewModel.ShowCurrencyCommand.Execute();
                }
            };

            _tipAmountTextView.FocusChange += (sender, e) =>
            {
                // Should be an End Edit binding on TipAmount EditText:
                if (!e.HasFocus)
                {
                    ViewModel.ShowCurrencyCommand.Execute();
                }

                // Should be a Start Edit binding on TipAmount EditText:
                if (e.HasFocus)
                {
                    ViewModel.ToggleToTipCustom.Execute();                    
                }
            };

            // Should be replaced with a click in layout binding
            _tipAmountTextView.Click += (sender, e) =>
            {
                ViewModel.ClearTipCommand.Execute();
            };

            // Should be a Click binding on a subcontrol (Behavior: Clicking anywhere on tip picker should trigger the ToggleToTipSelector)
            // Ok, done above
            // Should remove/migrate this code
            _tipPicker.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
                ViewModel.ToggleToTipSelector.Execute();
                //_tipAmountTextView.HideKeyboard();
            };
        }

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Payments_BookPayment);
        }
    }
}