using System;
using Android.Widget;
using Android.Runtime;
using Android.Content;
using Android.Util;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Text;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class EditTextLeftImage: LinearLayout
    {
		TextView _textViewLabel;
		private ImageView _imageLeftView;

		[Register(".ctor", "(Landroid/content/Context;)V", "")]
		public EditTextLeftImage(Context context): base(context)
		{
		}

		[Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
		public EditTextLeftImage(Context context, IAttributeSet attrs): base(context, attrs)
		{
		}

		public EditTextLeftImage(Context context, IAttributeSet attrs, int defStyle): base(context, attrs)
		{
		}

		public EditTextLeftImage(IntPtr ptr, Android.Runtime.JniHandleOwnership handle): base(ptr, handle)
		{
		}

		public event EventHandler<AfterTextChangedEventArgs> AfterCreditCardNumberChanged;

		private string _text;
		public string CreditCardNumber 
		{
			get {return _text;}
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

		private string _leftImage;
		public string LeftImage
		{
			get
			{
				return _leftImage;
			}
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
					;
				}
			}
		}


		protected override void OnFinishInflate ()
		{

			base.OnFinishInflate();
			var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
			View layout = inflater.Inflate(Resource.Layout.Control_EditTextLeftImage, this, true);

			_textViewLabel = (TextView) layout.FindViewById( Resource.Id.CreditCardNumber );
			_textViewLabel.Text = CreditCardNumber;
			_textViewLabel.AfterTextChanged += HandleCreditCardNumberChanged;

			_imageLeftView = (ImageView) layout.FindViewById( Resource.Id.CreditCardImagePath);
			if (LeftImage != null)
			{
				var resource = Resources.GetIdentifier(LeftImage.ToLower(), "drawable", Context.PackageName);
				if (resource != 0)
				{
					_imageLeftView.SetImageResource(resource);
					_textViewLabel.SetPadding(70.ToPixels(), 0, 0, 0);
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

