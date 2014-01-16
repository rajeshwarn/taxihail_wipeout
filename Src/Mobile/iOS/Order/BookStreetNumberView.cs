using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
    public partial class BookStreetNumberView : MvxViewController
    {
        public BookStreetNumberView() 
			: base("BookStreetNumberView", null)
        {
        }

		public new BookStreetNumberViewModel ViewModel
		{
			get
			{
				return (BookStreetNumberViewModel)DataContext;
			}
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

            var set = this.CreateBindingSet<BookStreetNumberView, BookStreetNumberViewModel>();

            set.Bind(txtStreetNumber)
                .For(v => v.Text)
                .To(vm => vm.StreetNumberOrBuildingName);

            set.Bind(lblStreetName)
                .For(v => v.Text)
                .To(vm => vm.StreetCity);

            set.Bind(btnPlaces)
                .For("TouchUpInside")
                .To(vm => vm.NavigateToPlaces);

            set.Bind(btnClear)
                .For("TouchUpInside")
                .To(vm => vm.DeleteAddressCommand);

            set.Bind(btnSearch)
                .For("TouchUpInside")
                .To(vm => vm.NavigateToSearch);

            set.Apply();
            
            ViewModel.Load();
            View.ApplyAppFont ();
        }
		
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
			NavigationItem.Title = Localize.GetValue("StreetNumberTitle");
        }

    }
}

