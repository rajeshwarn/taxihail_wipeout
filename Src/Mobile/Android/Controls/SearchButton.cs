using System;
using System.Windows.Input;
using Android.Content;
using Android.Runtime;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class SearchButton : LinearLayout
    {
        private Button _button;
        private string _text;
        private ITransformationMethod _transformationMethod;

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public SearchButton(Context context)
            : base(context)
        {
            Initialize();
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public SearchButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();
        }

        public SearchButton(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
            Initialize();
        }

// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand NavigateCommand { get; set; }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (_button != null) _button.Text = value;
            }
        }

        public ITransformationMethod TransformationMethod
        {
            get { return _transformationMethod; }
            set
            {
                _transformationMethod = value;
                if (_button != null) _button.TransformationMethod = value;
            }
        }

        private void Initialize()
        {
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_SearchButton, this, true);
            _button = (Button) layout.FindViewById(Resource.Id.navigateButton);

            if (_text != null) _button.Text = _text;
            if (_transformationMethod != null) _button.TransformationMethod = TransformationMethod;


            _button.Click += (sender, e) =>
            {
                if (NavigateCommand != null)
                {
                    NavigateCommand.Execute();
                }
            };
        }
    }
}