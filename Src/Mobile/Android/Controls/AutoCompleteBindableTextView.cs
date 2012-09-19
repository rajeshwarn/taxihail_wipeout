using System;
using Android.Content;
using Android.Util;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class AutoCompleteBindableTextView : AutoCompleteTextView
    {
          public AutoCompleteBindableTextView(Context context)
            : base(context)
        {
        }

        public AutoCompleteBindableTextView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public AutoCompleteBindableTextView(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {
        }

        protected override void OnTextChanged(Java.Lang.ICharSequence text, int start, int before, int after)
        {
            base.OnTextChanged(text, start, before, after);
            if ((TextChangedCommand != null) && (TextChangedCommand.CanExecute()))
            {
                TextChangedCommand.Execute();
            }
        }

        public IMvxCommand TextChangedCommand { get; set; }

    }
}