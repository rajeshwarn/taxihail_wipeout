using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class BindableRadioButton : RadioButton
    {
        protected BindableRadioButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize();
        }

        public BindableRadioButton(Context context) : base(context)
        {
            Initialize();
        }

        public BindableRadioButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public BindableRadioButton(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize();
        }

// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IMvxCommand SelectedChangedCommand { get; set; }

        private void Initialize()
        {
            Click += delegate
            {
                Selected = !Selected;
                if (Selected
                    && SelectedChangedCommand != null
                    && SelectedChangedCommand.CanExecute())
                {
                    SelectedChangedCommand.Execute(Tag);
                }
            };
        }
    }
}