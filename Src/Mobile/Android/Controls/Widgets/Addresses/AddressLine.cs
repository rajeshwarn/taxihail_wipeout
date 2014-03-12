using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
    //todo rename to AddressLineView
    public sealed class AddressLine : LinearLayout
    {
        private Button _button;
        private ImageView _imageView;
        private TextView _nameTextView;
        private TextView _addressTextView;
        private Action<AddressViewModel> _select;

        public AddressViewModel _address;

        public AddressLine(Context c, AddressViewModel address, Action<AddressViewModel> select) : base(c)
        {
            _address = address;
            _select = select;

            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_AddressLine, this, true);

            _button = layout.FindViewById<Button>(Resource.Id.CellButton);         
            _imageView = layout.FindViewById<ImageView>(Resource.Id.ImageView); 
            _nameTextView = layout.FindViewById<TextView>(Resource.Id.NameTextView); 
            _addressTextView = layout.FindViewById<TextView>(Resource.Id.AddressTextView);

            Initialize();
        }

        void Initialize()
        {
            _nameTextView.Text = _address.Address.FriendlyName;
            _addressTextView.Text = _address.Address.FullAddress;

            if (string.IsNullOrWhiteSpace(_address.Address.FriendlyName))
            {
                _nameTextView.Visibility = ViewStates.Gone;
            }

            var imageSrc = AddressTypeToDrawableConverter.GetDrawable(_address.Type);
            _imageView.SetImageResource(imageSrc);

            _button.Click += (sender, args) => _select(_address);
        }
    }
}