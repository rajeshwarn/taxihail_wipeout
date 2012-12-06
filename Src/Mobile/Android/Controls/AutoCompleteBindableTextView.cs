using System;
using System.Reactive.Linq;
using Android.Content;
using Android.Text;
using Android.Util;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Common.Extensions;

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
            

            
        }

        private IDisposable _subscription;
        protected override void OnVisibilityChanged(Android.Views.View changedView, Android.Views.ViewStates visibility)
        {
            base.OnVisibilityChanged(changedView, visibility);

            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            if (visibility == Android.Views.ViewStates.Visible)
            {
                var subsciption = Observable.FromEvent<TextChangedEventArgs>(
                            ev => this.TextChanged += (sender2, e2) => { ev(e2); },
                            ev => this.TextChanged -= (sender3, e3) => { ev(e3); }).Select(x =>x.Text.ToString());

                _subscription = subsciption.Throttle(TimeSpan.FromMilliseconds(700)).Subscribe(ExecuteCommand);

                if (Text.HasValue())
                {
                    ExecuteCommand(Text);
                }
            }
        }

        private void ExecuteCommand(string text)
        {
            if ((TextChangedCommand != null) && (TextChangedCommand.CanExecute()))
            {
                TextChangedCommand.Execute(text);
            }
        }

        public IMvxCommand TextChangedCommand { get; set; }

    }
}