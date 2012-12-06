﻿using System;
using System.Reactive.Linq;
using Android.Content;
using Android.Text;
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
            Init();
        }

        public AutoCompleteBindableTextView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Init();
        }

        public AutoCompleteBindableTextView(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {
            Init();
        }

        private void Init()
        {
            var subsciption = Observable.FromEvent<TextChangedEventArgs>(
           ev => this.TextChanged += (sender2, e2) => { ev(e2); },
           ev => this.TextChanged -= (sender3, e3) => { ev(e3); }).Select(e=>e.Text.ToString()).Where(txt=>txt.Length>0).Throttle(TimeSpan.FromMilliseconds(700));

            subsciption.Subscribe(ExecuteCommand);
        }



        private void ExecuteCommand(string text)
        {
            if ((TextChangedCommand != null) && (TextChangedCommand.CanExecute()))
            {
                TextChangedCommand.Execute(text != null ? text : null);
            }
        }

        public IMvxCommand TextChangedCommand { get; set; }

    }
}