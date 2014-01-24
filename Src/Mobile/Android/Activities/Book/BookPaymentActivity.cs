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
        private LinearLayout _layoutRoot;
        private EditText _meterAmountTextView;
        private EditText _tipAmountTextView;
        private EditTextSpinner _tipPicker;
        private TextView _totalAmountTextView;

        public double TipAmount
        {
            get { return CultureProvider.ParseCurrency( _tipAmountTextView.Text); }
            set { _tipAmountTextView.Text = CultureProvider.FormatCurrency(value); }
        }

        public double MeterAmount
        {
            get { return CultureProvider.ParseCurrency(_meterAmountTextView.Text); }
            set { _meterAmountTextView.Text = CultureProvider.FormatCurrency(value); }
        }

        public double TotalAmount
        {
            get { return CultureProvider.ParseCurrency(_totalAmountTextView.Text); }
            set { _totalAmountTextView.Text = CultureProvider.FormatCurrency(value); }
        }

        protected override void OnStart()
        {
            base.OnStart();

            _layoutRoot = FindViewById<LinearLayout>(Resource.Id.layoutRoot);
            _tipPicker = FindViewById<EditTextSpinner>(Resource.Id.tipPicker);
            _tipAmountTextView = FindViewById<EditText>(Resource.Id.tipAmountTextView);
            _meterAmountTextView = FindViewById<EditText>(Resource.Id.meterAmountTextView);
            _totalAmountTextView = FindViewById<TextView>(Resource.Id.totalAmountTextView);

            UpdateAmounts();

            _tipAmountTextView.TextChanged += (sender, e) => UpdateAmounts();
            _meterAmountTextView.FocusChange += (sender, e) =>
            {
                if (!e.HasFocus)
                {
                    MeterAmount = MeterAmount;
                }
            };

            _meterAmountTextView.TextChanged += (sender, e) =>
            {
                TipAmount = MeterAmount*GetTip();
                UpdateAmounts();
            };

            _meterAmountTextView.FocusChange += (sender, e) =>
            {
                if (!e.HasFocus)
                {
                    TipAmount = MeterAmount * (GetTip());
                    UpdateAmounts();
                }
            };

            _tipAmountTextView.EditorAction += (sender, e) =>
            {
                e.Handled = false;
                TipAmount = TipAmount; //format	
            };

            _tipAmountTextView.FocusChange += (sender, e) =>
            {
                if (!e.HasFocus)
                {
                    TipAmount = TipAmount; //format	
                }
            };

            _meterAmountTextView.EditorAction += (sender, e) =>
            {
                e.Handled = false;
                MeterAmount = MeterAmount; //format				
            };

            _tipPicker.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {

                TipAmount = MeterAmount * GetTip();
                _tipAmountTextView.HideKeyboard(this);
                _layoutRoot.RequestFocus();
                UpdateAmounts();

            };
                       
        }

        public double GetTip()
        {
            return (double)ViewModel.PaymentPreferences.Tip / 100.00;
        }

        public void UpdateAmounts()
        {
            TotalAmount = MeterAmount + TipAmount;
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Payments_BookPayment);
            ViewModel.OnViewLoaded();
        }
    }
}