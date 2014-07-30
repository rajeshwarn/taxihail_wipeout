using System;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class EditTextLeftImage : LinearLayout
    {
        private ImageView _imageLeftView;
        private string _leftImage;
        private string _text;
        private TextView _textViewLabel;

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public EditTextLeftImage(Context context) : base(context)
        {
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public EditTextLeftImage(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public string CreditCardNumber
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    if (_textViewLabel != null)
                        _textViewLabel.Text = value;
                }
            }
        }

        public string LeftImage
        {
            get { return _leftImage; }
            set
            {
                if (_leftImage != value)
                {
                    _leftImage = value;
                    if (_imageLeftView != null)
                    {
                        var resource = Resources.GetIdentifier(_leftImage.ToLower(), "drawable", Context.PackageName);
                        if (resource != 0)
                            _imageLeftView.SetImageResource(resource);
                    }
                    
                }
            }
        }

        public event EventHandler<AfterTextChangedEventArgs> AfterCreditCardNumberChanged;

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            View layout = inflater.Inflate(Resource.Layout.Control_EditTextLeftImage, this, true);

            _textViewLabel = (TextView) layout.FindViewById(Resource.Id.CreditCardNumber);
            _textViewLabel.Text = CreditCardNumber;
            _textViewLabel.AfterTextChanged += HandleCreditCardNumberChanged;
            _textViewLabel.InputType = InputTypes.ClassNumber;
            _textViewLabel.SetSelectAllOnFocus (true);

            _imageLeftView = (ImageView) layout.FindViewById(Resource.Id.CreditCardImagePath);
            if (LeftImage != null)
            {
                var resource = Resources.GetIdentifier(LeftImage.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
                    _imageLeftView.SetImageResource(resource);
					if (this.Services ().Localize.IsRightToLeft) {
						_textViewLabel.SetPadding (0, 0, 50.ToPixels (), 0);
					} else {
						_textViewLabel.SetPadding (50.ToPixels (), 0, 0, 0);
					}
                }
            }
        }

        private void HandleCreditCardNumberChanged(object sender, AfterTextChangedEventArgs e)
        {
            _text = e.Editable.ToString();

            if (AfterCreditCardNumberChanged != null)
            {
                AfterCreditCardNumberChanged.Invoke(this, e);
            }
        }
    }
}