using System;
using Android.Content;
using Android.Runtime;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;

using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class CreditCardButton : LinearLayout
    {
        private ImageView _cardImage;
        private string _creditCardCompany;
        private EditText _editText;
        private string _last4Digits;

        private TextView _last4DigitsTextView;
        private string _text;
        private ITransformationMethod _transformationMethod;

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public CreditCardButton(Context context)
            : base(context)
        {
            Initialize();
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public CreditCardButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();
        }

        public CreditCardButton(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
            Initialize();
        }

// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IMvxCommand NavigateCommand { get; set; }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (_editText != null) _editText.Text = value;
            }
        }

        public string CreditCardCompany
        {
            set
            {
                _creditCardCompany = value;
                SetCreditCardImage();
            }
        }

        public string Last4Digits
        {
            set
            {
                _last4Digits = value;
                if (_last4DigitsTextView != null)
                {
                    _last4DigitsTextView.Text = string.IsNullOrEmpty(value)
                        ? string.Empty
                        : "\u2022\u2022\u2022\u2022 " + value;
                }
            }
        }

        public ITransformationMethod TransformationMethod
        {
            get { return _transformationMethod; }
            set
            {
                _transformationMethod = value;
                if (_editText != null) _editText.TransformationMethod = value;
            }
        }

        private void Initialize()
        {
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_CreditCardButton, this, true);
            _editText = (EditText) layout.FindViewById(Resource.Id.creditCardName);
            if (_text != null)
                _editText.Text = _text;
            if (_transformationMethod != null)
                _editText.TransformationMethod = TransformationMethod;

            _last4DigitsTextView = (TextView) layout.FindViewById(Resource.Id.creditCardLast4Digits);
            if (_last4Digits != null)
            {
                _last4DigitsTextView.Text = _last4Digits;
            }
            _cardImage = (ImageView) layout.FindViewById(Resource.Id.creditCardImage);
            SetCreditCardImage();

            var button = (Button) layout.FindViewById(Resource.Id.creditCardButton);
            button.Click += (sender, e) =>
            {
                if (NavigateCommand != null)
                {
                    NavigateCommand.Execute();
                }
            };
        }

        private void SetCreditCardImage()
        {
            if (_creditCardCompany != null)
            {
                var resource = Resources.GetIdentifier(_creditCardCompany.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
                    _cardImage.SetImageResource(resource);
                    _cardImage.Visibility = ViewStates.Visible;
                }
                else
                {
                    _cardImage.Visibility = ViewStates.Gone;
                }
            }
            else
            {
                _cardImage.Visibility = ViewStates.Gone;
            }
        }
    }
}