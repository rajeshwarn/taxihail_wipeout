
using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.ViewModels;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Navigation;

namespace apcurium.MK.Booking.Mobile.Client
{
    [NoHistory]
    public partial class BookStreetNumberView : MvxBindingTouchViewController<BookStreetNumberViewModel>
    {
        public BookStreetNumberView() 
            : base(new MvxShowViewModelRequest<BookStreetNumberViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
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
            lblRefineAddress.Text = Resources.GetValue("StreetNumberLabel");
            lblStreetName.Font = AppStyle.BoldTextFont;
            lblStreetName.TextColor = AppStyle.DarkText;
            AppButtons.FormatStandardButton((GradientButton)btnSearch, Resources.GetValue("StreetNumberSearchBt"), AppStyle.ButtonColor.Grey, "Assets/Search/SearchIcon.png", "Assets/Cells/rightArrow.png");
            AppButtons.FormatStandardButton((GradientButton)btnPlaces, Resources.GetValue("StreetNumberPlacesBt"), AppStyle.ButtonColor.Grey, "Assets/Search/SearchPlace.png", "Assets/Cells/rightArrow.png");

            AppButtons.FormatStandardButton((GradientButton)btnClear, Resources.GetValue("DeleteAddressBt"), AppStyle.ButtonColor.Red, "Assets/Search/cancel.png", null);

            ((GradientButton)btnSearch).HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            ((GradientButton)btnPlaces).HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            ((GradientButton)btnClear).HorizontalAlignment = UIControlContentHorizontalAlignment.Left;

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

            var button = new MonoTouch.UIKit.UIBarButtonItem(Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
                ViewModel.SaveCommand.Execute();
            });
            NavigationItem.RightBarButtonItem = button;

            this.AddBindings(new Dictionary<object, string>() {
                { txtStreetNumber, "{'Text': {'Path': 'StreetNumberOrBuildingName'}}"},
                { lblStreetName, "{'Text': {'Path': 'StreetCity'}}"},               
                { btnPlaces, "{'TouchUpInside': {'Path': 'NavigateToPlaces'}}" },
                { btnClear, "{'TouchUpInside': {'Path': 'DeleteAddressCommand'}}" },
                { btnSearch, "{'TouchUpInside': {'Path': 'NavigateToSearch'}}" }
            });
            
            ViewModel.Load();
            this.View.ApplyAppFont ();
        }
		
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("StreetNumberTitle"), true);
        }

    }
}

