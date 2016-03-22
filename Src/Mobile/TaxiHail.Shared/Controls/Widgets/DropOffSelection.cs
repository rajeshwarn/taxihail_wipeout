using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Runtime;
using Android.Content;
using Android.Util;
using Cirrious.MvvmCross.Binding.BindingContext;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.DropOffSelection")]
    public class DropOffSelection : MvxFrameControl
    {
        private AddressTextBox _viewDestination;

        private bool _isShown = true;
        private ViewStates _animatedVisibility;

        public Button BigInvisibleButton { get; set; }

        private const int HiddenHeightNoAnim = -500;
        private const int HiddenHeightOffset = -50;

        public DropOffSelection(Context context, IAttributeSet attrs) : base (Resource.Layout.SubView_DropOffSelection, context, attrs)
        {
            this.DelayBind(() => 
                {
                    _viewDestination = Content.FindViewById<AddressTextBox>(Resource.Id.viewDestination);
                    _viewDestination.IsDestination = true;
                    _viewDestination.SetInvisibleButton(BigInvisibleButton);
                   
                    _viewDestination.IsSelected = true;
                    _viewDestination.UserInputDisabled = false;

                    InitializeBinding();
                });
        }

        private OrderOptionsViewModel ViewModel { get { return (OrderOptionsViewModel)DataContext; } }

        public ViewStates AnimatedVisibility
        {
            get { return _animatedVisibility; }
            set
            {
                _animatedVisibility = value;
                if (value == ViewStates.Visible)
                {
                    ShowIfNeeded();
                    return;
                }
                HideIfNeeded();
            }
        }

        public void HideWithoutAnimation()
        {
            _isShown = false;

            ((MarginLayoutParams)LayoutParameters).TopMargin = HiddenHeightNoAnim;
        }

        private void HideIfNeeded()
        {
            if (!_isShown || Height == 0)
            {
                return;
            }

            _isShown = false;

            var translationOffset = -Height + HiddenHeightOffset;

            StartAnimation(AnimationHelper.GetForYTranslation(this, translationOffset));
        }

        private void ShowIfNeeded()
        {
            if (_isShown || Height == 0)
            {
                return;
            }

            _isShown = true;

            var translationOffset = -Height + HiddenHeightOffset;

            ((MarginLayoutParams)LayoutParameters).TopMargin = translationOffset;

            StartAnimation(AnimationHelper.GetForYTranslation(this, 0));
        }

        private void InitializeBinding()
        {

            _viewDestination.AddressUpdated = streetNumber => {
                ViewModel.DestinationAddress.ChangeStreetNumber(streetNumber);
                ViewModel.SetAddress.Execute (ViewModel.DestinationAddress);
            };

            var set = this.CreateBindingSet<DropOffSelection, DropOffSelectionMidTripViewModel> ();

            set.Bind (_viewDestination)
                .For (v => v.IsLoadingAddress)
                .To (vm => vm.IsLoadingAddress);

            set.Bind (_viewDestination)
                .For (v => v.CurrentAddress)
                .To (vm => vm.DestinationAddress);

            set.Bind (_viewDestination)
                .For ("AddressClicked")
                .To (vm => vm.ShowDestinationSearchAddress);

            set.Apply ();
        }
    }
}


