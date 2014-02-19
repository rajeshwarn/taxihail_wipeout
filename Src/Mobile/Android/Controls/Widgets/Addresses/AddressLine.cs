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
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
    //todo rename to AddressLineView
    public sealed class AddressLine : MvxFrameControl
    {
        public AddressViewModel _address;

        public AddressLine(Context c, AddressViewModel address, Action<AddressViewModel> select)
            : base(Resource.Layout.Control_AddressLine, c, null)
        {
            this.DelayBind(() =>
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

                _button = FindViewById<Button>(Resource.Id.CellButton);         

                _imageView = FindViewById<ImageView>(Resource.Id.ImageView); 

                _nameTextView = FindViewById<TextView>(Resource.Id.NameTextView); 

                _addressTextView = FindViewById<TextView>(Resource.Id.AddressTextView);

                _address = address;

                _nameTextView.Text = _address.Address.FriendlyName;
                _addressTextView.Text = _address.Address.FullAddress;

                if (string.IsNullOrWhiteSpace(_address.Address.FriendlyName))
                {
                    _nameTextView.Visibility = ViewStates.Gone;
                }

                var imageSrc = AddressTypeToDrawableConverter.GetDrawable(_address.Type);
                _imageView.SetImageResource(imageSrc);

                _button.Click += (sender, args) => select(_address);
            });
        }

        private Button _button;

        private ImageView _imageView;

        private TextView _nameTextView;

        private TextView _addressTextView;
    }
}