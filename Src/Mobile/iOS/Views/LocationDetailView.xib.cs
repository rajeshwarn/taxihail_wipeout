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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

            NavigationItem.HidesBackButton = false;
            NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_LocationDetail"), true);
            
            

            lblName.Text = Resources.LocationDetailGiveItANameLabel;

            ((TextField)txtAddress).PaddingLeft = 3;
            ((TextField)txtAptNumber).PaddingLeft = 3;
            ((TextField)txtRingCode).PaddingLeft = 3;
            ((TextField)txtName).PaddingLeft = 3;

            txtAddress.Placeholder = Resources.LocationDetailStreetAddressPlaceholder;
            txtAptNumber.Placeholder = Resources.LocationDetailAptPlaceholder;
            txtRingCode.Placeholder = Resources.LocationDetailRingCodePlaceholder;
            txtName.Placeholder = Resources.LocationDetailGiveItANamePlaceholder;

            txtAddress.ShouldReturn = HandleShouldReturn;
            txtAptNumber.ShouldReturn = HandleShouldReturn;
            txtRingCode.ShouldReturn = HandleShouldReturn;
            txtName.ShouldReturn = HandleShouldReturn;


            AppButtons.FormatStandardButton((GradientButton)btnSave, Resources.SaveButton, AppStyle.ButtonColor.Green); 
			(btnBook).SetTitle(Resources.GetValue("LocationDetailRebookButton"), UIControlState.Normal);
            AppButtons.FormatStandardButton((GradientButton)btnDelete, Resources.DeleteButton, AppStyle.ButtonColor.Red); 

            if ( !ViewModel.ShowRingCodeField )
            {
                txtRingCode.Hidden = true;
                txtAptNumber.Frame = new RectangleF( txtAptNumber.Frame.X, txtAptNumber.Frame.Y, txtAddress.Frame.Width, txtAptNumber.Frame.Height );
            }

            this.AddBindings(new Dictionary<object, string>{
                { txtAddress, "{'Text': {'Path': 'BookAddress'}, 'Ended': {'Path': 'ValidateAddress'}}" },
                { txtAptNumber, "{'Text': {'Path': 'Apartment'}}" },
                { txtRingCode, "{'Text': {'Path': 'RingCode'}}" },
                { txtName, "{'Text': {'Path': 'FriendlyName'}}" },
                { btnSave, "{'TouchUpInside': {'Path': 'SaveAddress'}}" },
				{ btnBook, "{'TouchUpInside': {'Path': 'RebookOrder'}, 'Hidden': {'Path': 'RebookIsAvailable', 'Converter':'BoolInverter'}}" },
                { btnDelete, "{'TouchUpInside': {'Path': 'DeleteAddress'}, 'Hidden': {'Path': 'IsNew'}}" },
            });

            btnSave.Hidden = true;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(  Resources.SaveButton , UIBarButtonItemStyle.Plain, (s, e) => ViewModel.SaveAddress.Execute () );

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

