
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class DropOffSelectionControl : BaseBindableChildView<DropOffSelectionMidTripViewModel>
    {
        private NSLayoutConstraint _heightConstraint;

        public DropOffSelectionControl (IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;
            viewDestination.BackgroundColor = UIColor.Clear;
            viewDestination.IsSelected = true;
            viewDestination.UserInputDisabled = false;
        }

        private void InitializeBinding()
        {
            viewDestination.AddressUpdated = streetNumber => {
                ViewModel.DestinationAddress.ChangeStreetNumber(streetNumber);
                ViewModel.SetAddress.ExecuteIfPossible(ViewModel.DestinationAddress);
            };

            var set = this.CreateBindingSet<DropOffSelectionControl, DropOffSelectionMidTripViewModel>();

            set.Bind(viewDestination)
                .For(v => v.IsLoadingAddress)
                .To(vm => vm.IsLoadingAddress);

            set.Bind(viewDestination.AddressTextView)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            set.Bind(viewDestination.AddressButton)
                .For(ve => ve.AccessibilityLabel)
                .To(vm => vm.DestinationAddress.DisplayAddress);

            viewDestination.AddressButton.AccessibilityHint = Localize.GetValue("DestinationTextPlaceholder");
            
            set.Bind(viewDestination)
                .For("AddressClicked")
                .To(vm => vm.ShowDestinationSearchAddress);

            set.Apply();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var nib = UINib.FromName ("DropOffSelectionControl", null);
            var view = (UIView)nib.Instantiate(this, null)[0];
            AddSubview(view);

            Initialize();

            this.DelayBind (() => {
                InitializeBinding();
            });
        }
    }
}

