using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
    public class AddressBar : MvxFrameControl
    {
        public AddressBar(Context context, IAttributeSet attrs)
            : base (Resource.Layout.SubView_OrderOptions, context, attrs)
        {

        }
        private AddressSelectionMode _addressSelectionMode;
        public AddressSelectionMode AddressSelectionMode
        {
            get { return _addressSelectionMode; }
            set
            {
                _addressSelectionMode = value;

                if (value == AddressSelectionMode.PickupSelection)
                {
                    DestinationSection.Visibility = ViewStates.Gone;
                    AddressTextBox.IsReadOnly = false;
                }
                else
                {
                    DestinationSection.Visibility = ViewStates.Visible;
                    AddressTextBox.IsReadOnly = true;
                }
            }
        }

        private Address _pickupAddress;
        public Address PickupAddress
        {
            get { return _pickupAddress; }
            set
            {
                _pickupAddress = value;
                AddressTextBox.Address = value.DisplayAddress;
            }
        }

        private Address _dropOffAddress;
        public Address DropOffAddress
        {
            get { return _dropOffAddress; }
            set
            {
                _dropOffAddress = value;
                DestinationAddressTextBox.Address = value.DisplayAddress;
            }
        }

        public AddressTextBox AddressTextBox
        {
            get { return (AddressTextBox)FindViewById<AddressTextBox>(Resource.Id.AddressTextBox); }
        }

        public AddressTextBox DestinationAddressTextBox
        {
            get { return (AddressTextBox)FindViewById<AddressTextBox>(Resource.Id.DestinationAddressTextBox); }
        }

        private LinearLayout DestinationSection
        {
            get { return (LinearLayout)FindViewById<LinearLayout>(Resource.Id.DestinationSection); }
        }
        private TextView TariffEstimationTextView
        {
            get { return (TextView)FindViewById<TextView>(Resource.Id.TariffEstimationTextView); }
        }

        bool _isLoadingAddress;
        public bool IsLoadingAddress
        {
            get
            {
                return _isLoadingAddress;
            }
            set{
                DestinationAddressTextBox.IsLoadingAddress  = value;
                AddressTextBox.IsLoadingAddress = value;
                _isLoadingAddress = value;
            }
        }

        public string FareEstimate
        {
            get { return TariffEstimationTextView.Text; }
            set
            {
                TariffEstimationTextView.Visibility = string.IsNullOrWhiteSpace(value) ? ViewStates.Gone : ViewStates.Visible;
                TariffEstimationTextView.Text = value;
            }
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            AddressTextBox.Hint = "";
            DestinationAddressTextBox.Hint = "";
        }

    }
}