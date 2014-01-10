using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
    [NoHistory]
    public partial class BookStreetNumberView : MvxBindingTouchViewController<BookStreetNumberViewModel>
    {
        public BookStreetNumberView() 
            : base(new MvxShowViewModelRequest<BookStreetNumberViewModel>( null, true, new MvxRequestedBy()   ) )
        {
        }
        
        public BookStreetNumberView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public BookStreetNumberView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
		
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;
        }
		
      

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();		

            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            lblRefineAddress.Font = AppStyle.NormalTextFont;
            lblRefineAddress.Text = Localize.GetValue("StreetNumberLabel");
            lblStreetName.Font = AppStyle.BoldTextFont;
            lblStreetName.TextColor = AppStyle.DarkText;
            AppButtons.FormatStandardButton((GradientButton)btnSearch, Localize.GetValue("StreetNumberSearchBt"), AppStyle.ButtonColor.Grey, "Assets/Search/SearchIcon.png", "Assets/Cells/rightArrow.png");
            AppButtons.FormatStandardButton(btnPlaces, Localize.GetValue("StreetNumberPlacesBt"), AppStyle.ButtonColor.Grey, "Assets/Search/SearchPlace.png", "Assets/Cells/rightArrow.png");

            AppButtons.FormatStandardButton(btnClear, Localize.GetValue("DeleteAddressBt"), AppStyle.ButtonColor.Red, "Assets/Search/cancel.png");

            btnSearch.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            btnPlaces.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            btnClear.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;

            txtStreetNumber.ShouldReturn = delegate {
                txtStreetNumber.ResignFirstResponder();
                return true;
            };


            txtStreetNumber.ShouldChangeCharacters = (textField, range, replacementString) => 
            {
                using (NSString original = new NSString(textField.Text), replace = new NSString(replacementString.ToUpper()))
                {
                    return original.Replace (range, replace).Length <= ViewModel.NumberOfCharAllowed;
                }
                //return false;
            };

            var button = new UIBarButtonItem(Localize.GetValue("DoneButton"), UIBarButtonItemStyle.Plain, delegate
            {
                ViewModel.SaveCommand.Execute();
            });
            NavigationItem.RightBarButtonItem = button;

            this.AddBindings(new Dictionary<object, string> {
                { txtStreetNumber, "{'Text': {'Path': 'StreetNumberOrBuildingName'}}"},
                { lblStreetName, "{'Text': {'Path': 'StreetCity'}}"},               
                { btnPlaces, "{'TouchUpInside': {'Path': 'NavigateToPlaces'}}" },
                { btnClear, "{'TouchUpInside': {'Path': 'DeleteAddressCommand'}}" },
                { btnSearch, "{'TouchUpInside': {'Path': 'NavigateToSearch'}}" }
            });
            
            ViewModel.Load();
            View.ApplyAppFont ();
        }
		
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            NavigationItem.TitleView = new TitleView(null, Localize.GetValue("StreetNumberTitle"), true);
        }

    }
}

