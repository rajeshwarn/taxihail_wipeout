using Android.App;
using Android.Content.PM;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation
{
	[Activity(Theme = "@style/MainTheme",
		ScreenOrientation = ScreenOrientation.Portrait
	)]
	public class AddressPickerContainerActivity : BaseBindingActivity<AddressPickerViewModel>
	{
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

			SetContentView(Resource.Layout.View_AddressPickerContainer);

			var addressPicker = FindViewById<AddressPicker>(Resource.Id.searchAddressControl);
			addressPicker.Bind(this, "DataContext;");
			addressPicker.DelayBind(() => addressPicker.FocusOnTextField ());
		}
	}
}

