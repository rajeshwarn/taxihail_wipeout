using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
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

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.HidesBackButton = false;
			NavigationItem.Title = Localize.GetValue("View_LocationDetail");

            ChangeThemeOfBarStyle();
            ChangeRightBarButtonFontToBold();
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

            txtAptNumber.ShouldReturn = HandleShouldReturn;
            txtRingCode.ShouldReturn = HandleShouldReturn;
            txtName.ShouldReturn = HandleShouldReturn;

            txtName.Placeholder = Localize.GetValue("LocationDetailNameLabel");;
            txtName.AccessibilityLabel = txtName.Placeholder;

            txtAddress.Placeholder = Localize.GetValue("LocationDetailAddressLabel");
            txtAddress.AccessibilityLabel = txtAddress.Placeholder;

            txtAptNumber.Placeholder = Localize.GetValue("LocationDetailApartmentLabel");
            txtAptNumber.AccessibilityLabel = txtAptNumber.Placeholder;

            txtRingCode.Placeholder = Localize.GetValue("LocationDetailRingCodeLabel");
            txtRingCode.AccessibilityLabel = txtRingCode.Placeholder;


            if ( !ViewModel.Settings.ShowRingCodeField )
            {
				lblRingCode.Hidden = true;
                txtRingCode.Hidden = true;

				lblRingCode.RemoveFromSuperview();
				txtRingCode.RemoveFromSuperview();
            }

			if (!ViewModel.Settings.ShowPassengerApartment)
			{
				lblRingCode.Hidden = true;
				txtRingCode.Hidden = true;
				lblApartment.Hidden = true;
				txtAptNumber.Hidden = true;

				lblRingCode.RemoveFromSuperview();
				txtRingCode.RemoveFromSuperview();
				lblApartment.RemoveFromSuperview();
				txtAptNumber.RemoveFromSuperview();
			}

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("Save"), UIBarButtonItemStyle.Plain, null);

			var set = this.CreateBindingSet<LocationDetailView, LocationDetailViewModel> ();

            set.Bind (NavigationItem.RightBarButtonItem)
                .For ("Clicked")
                .To(vm => vm.SaveAddress);

            set.Bind(txtAddress)
				.For(v => v.Text)
                .To(vm => vm.BookAddress)
                .OneWay();
			set.Bind(txtAddress)
				.For(v => v.NavigateCommand)
				.To(vm => vm.NavigateToSearch);

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
				.For("HiddenEx")
				.To(vm => vm.RebookIsAvailable)
				.WithConversion("BoolInverter");

			set.Bind(btnDelete)
				.For("TouchUpInside")
				.To(vm => vm.DeleteAddress);
			set.Bind(btnDelete)
                .For("HiddenEx")
				.To(vm => vm.IsNew);

			set.Apply();
        }

        bool HandleShouldReturn (UITextField textField)
        {
            return textField.ResignFirstResponder();
        }
    }
}

