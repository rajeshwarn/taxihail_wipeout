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
    public class EditTextNavigate : LinearLayout
    {
        private TextView _label;
        private string _text;
        private ITransformationMethod _transformationMethod;

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public EditTextNavigate(Context context)
            : base(context)
        {
            Initialize();
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public EditTextNavigate(Context context, IAttributeSet attrs)
            : base(context, attrs)
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
                if (_label != null) _label.Text = value;
            }
        }

        public ITransformationMethod TransformationMethod
        {
            get { return _transformationMethod; }
            set
            {
                _transformationMethod = value;
                if (_label != null) _label.TransformationMethod = value;
            }
        }

        private void Initialize()
        {
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_EditTextNavigate, this, true);

            _label = (TextView) layout.FindViewById(Resource.Id.label);
            _label.Focusable = false;
            if (_text != null) _label.Text = _text;
            if (_transformationMethod != null) _label.TransformationMethod = TransformationMethod;

            var button = (Button) layout.FindViewById(Resource.Id.navigateButton);
            button.Click += (sender, e) =>
            {
                if (NavigateCommand != null)
                {
                    NavigateCommand.Execute();
                }
            };
        }
    }
}