using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class LocationDetailView : BaseViewController<LocationDetailViewModel>
    {
		public LocationDetailView () : base("LocationDetailView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationItem.HidesBackButton = false;
			NavigationItem.Title = Localize.GetValue("View_LocationDetail");
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			FlatButtonStyle.Red.ApplyTo (btnDelete);
			FlatButtonStyle.Green.ApplyTo (btnRebook);

			lblName.Text = Localize.GetValue("LocationDetailNameLabel");
			lblAddress.Text = Localize.GetValue("LocationDetailAddressLabel");
			lblApartment.Text = Localize.GetValue("LocationDetailApartmentLabel");
			lblRingCode.Text = Localize.GetValue("LocationDetailRingCodeLabel");

			btnRebook.SetTitle (Localize.GetValue("Rebook"), UIControlState.Normal);
			btnDelete.SetTitle (Localize.GetValue("Delete"), UIControlState.Normal);

            txtAddress.ShouldReturn = HandleShouldReturn;
            txtAptNumber.ShouldReturn = HandleShouldReturn;
            txtRingCode.ShouldReturn = HandleShouldReturn;
            txtName.ShouldReturn = HandleShouldReturn;

            if ( !ViewModel.ShowRingCodeField )
            {
				lblRingCode.Hidden = true;
                txtRingCode.Hidden = true;
            }

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("Save"), UIBarButtonItemStyle.Plain, (s, e) => ViewModel.SaveAddress.Execute());

			var set = this.CreateBindingSet<LocationDetailView, LocationDetailViewModel> ();

			set.Bind(txtAddress)
				.For(v => v.Text)
				.To(vm => vm.BookAddress);
			set.Bind(txtAddress)
				.For("Ended")
				.To(vm => vm.ValidateAddress);

			set.Bind(txtAptNumber)
				.For(v => v.Text)
				.To(vm => vm.Apartment);

			set.Bind(txtRingCode)
				.For(v => v.Text)
				.To(vm => vm.RingCode);

			set.Bind(txtName)
				.For(v => v.Text)
				.To(vm => vm.FriendlyName);

			set.Bind(btnRebook)
				.For("TouchUpInside")
				.To(vm => vm.RebookOrder);
			set.Bind(btnRebook)
				.For(v => v.Hidden)
				.To(vm => vm.RebookIsAvailable)
				.WithConversion("BoolInverter");

			set.Bind(btnDelete)
				.For("TouchUpInside")
				.To(vm => vm.DeleteAddress);
			set.Bind(btnDelete)
				.For(v => v.Hidden)
				.To(vm => vm.IsNew);

			set.Apply();
        }

        public override void ViewWillDisappear (bool animated)
        {
            ViewModel.StopValidatingAddresses();
        }

        bool HandleShouldReturn (UITextField textField)
        {
            return textField.ResignFirstResponder();
        }
    }
}

