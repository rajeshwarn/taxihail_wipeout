using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class LocationDetailView : MvxViewController
    {
        public LocationDetailView () 
			: base("LocationDetailView", null)
		{
			Initialize();
		}

        void Initialize()
        {
        }

		public new LocationDetailViewModel ViewModel
		{
			get
			{
				return (LocationDetailViewModel)DataContext;
			}
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

            NavigationItem.HidesBackButton = false;
            NavigationItem.TitleView = new TitleView(null, Localize.GetValue("View_LocationDetail"), true);

            lblName.Text = Localize.GetValue("LocationDetailGiveItANameLabel");

            ((TextField)txtAddress).PaddingLeft = 3;
            ((TextField)txtAptNumber).PaddingLeft = 3;
            ((TextField)txtRingCode).PaddingLeft = 3;
            ((TextField)txtName).PaddingLeft = 3;

            txtAddress.Placeholder = Localize.GetValue("LocationDetailStreetAddressPlaceholder");
            txtAptNumber.Placeholder = Localize.GetValue("LocationDetailAptPlaceholder");
            txtRingCode.Placeholder = Localize.GetValue("LocationDetailRingCodePlaceholder");
            txtName.Placeholder = Localize.GetValue("LocationDetailGiveItANamePlaceholder");

            txtAddress.ShouldReturn = HandleShouldReturn;
            txtAptNumber.ShouldReturn = HandleShouldReturn;
            txtRingCode.ShouldReturn = HandleShouldReturn;
            txtName.ShouldReturn = HandleShouldReturn;

            AppButtons.FormatStandardButton((GradientButton)btnSave, Localize.GetValue("SaveButton"), AppStyle.ButtonColor.Green); 
			(btnBook).SetTitle(Localize.GetValue("LocationDetailRebookButton"), UIControlState.Normal);
            AppButtons.FormatStandardButton((GradientButton)btnDelete, Localize.GetValue("DeleteButton"), AppStyle.ButtonColor.Red); 

            if ( !ViewModel.ShowRingCodeField )
            {
                txtRingCode.Hidden = true;
                txtAptNumber.Frame = new RectangleF( txtAptNumber.Frame.X, txtAptNumber.Frame.Y, txtAddress.Frame.Width, txtAptNumber.Frame.Height );
            }

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

			set.Bind(btnSave)
				.For("TouchUpInside")
				.To(vm => vm.SaveAddress);

			set.Bind(btnBook)
				.For("TouchUpInside")
				.To(vm => vm.RebookOrder);
			set.Bind(btnBook)
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
            
            btnSave.Hidden = true;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("SaveButton"), UIBarButtonItemStyle.Plain, (s, e) => ViewModel.SaveAddress.Execute());

            View.ApplyAppFont ();
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

