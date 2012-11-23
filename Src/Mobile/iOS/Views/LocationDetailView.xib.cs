
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Diagnostic;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class LocationDetailView : MvxBindingTouchViewController<LocationDetailViewModel>
    {
        #region Constructors

        public LocationDetailView () 
			: base(new MvxShowViewModelRequest<LocationDetailViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
		{
			Initialize();
		}
		
		public LocationDetailView (MvxShowViewModelRequest request) 
			: base(request)
		{
			Initialize();
		}
		
		public LocationDetailView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{
			Initialize();
		}

        #endregion

        void Initialize()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

            this.NavigationItem.HidesBackButton = false;
            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_LocationDetail"), true);
            
            
            lblSaveAsAFavorite.Text = Resources.LocationDetailInstructionLabel;
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
            ((GradientButton)btnBook).SetTitle(Resources.BookItButton, UIControlState.Normal);
            AppButtons.FormatStandardButton((GradientButton)btnDelete, Resources.DeleteButton, AppStyle.ButtonColor.Red); 

            this.AddBindings(new Dictionary<object, string>{
                { txtAddress, "{'Text': {'Path': 'FullAddress'}, 'Ended': {'Path': 'ValidateAddress'}}" },
                { txtAptNumber, "{'Text': {'Path': 'Apartment'}}" },
                { txtRingCode, "{'Text': {'Path': 'RingCode'}}" },
                { txtName, "{'Text': {'Path': 'FriendlyName'}}" },
                { btnSave, "{'TouchUpInside': {'Path': 'SaveAddress'}}" },
                { btnBook, "{'TouchUpInside': {'Path': 'RebookOrder'}, 'Hidden': {'Path': 'IsNew'}}" },
                { btnDelete, "{'TouchUpInside': {'Path': 'DeleteAddress'}, 'Hidden': {'Path': 'IsNew'}}" },
            });

        }

        private bool HandleShouldReturn (UITextField textField)
        {
            return textField.ResignFirstResponder();
        }

       
    }
}

