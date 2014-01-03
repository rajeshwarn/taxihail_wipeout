using System;
using System.Reactive.Linq;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class AutoCompleteBindableTextView : AutoCompleteTextView
    {
        private IDisposable _subscription;
        private IDisposable _subscriptionTypeStart;

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

        public AutoCompleteBindableTextView(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
            Init();
        }

// ReSharper disable UnusedAutoPropertyAccessor.Global
        public IMvxCommand TextChangedCommand { get; set; }


        public IMvxCommand OnTypeStarted { get; set; }

        public bool IsAddressSearching { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
        private void Init()
        {
        }

        protected override void OnVisibilityChanged(View changedView, ViewStates visibility)
        {
            base.OnVisibilityChanged(changedView, visibility);

            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            if (_subscriptionTypeStart != null)
            {
                _subscriptionTypeStart.Dispose();
                _subscriptionTypeStart = null;
            }

            if (visibility == ViewStates.Visible)
            {
                var subsciption = Observable.FromEvent<TextChangedEventArgs>(
                    ev => TextChanged += (sender2, e2) => ev(e2),
// ReSharper disable once EventUnsubscriptionViaAnonymousDelegate
                    ev => TextChanged -= (sender3, e3) => ev(e3)).Select(x => x.Text.ToString());

                _subscription = subsciption.Throttle(TimeSpan.FromMilliseconds(700)).Subscribe(ExecuteCommand);
                _subscriptionTypeStart = subsciption.Subscribe(_ =>
                {
                    if (IsAddressSearching)
                    {
                        OnTypeStarted.Execute();
                    }
                });

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
    }
}