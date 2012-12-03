
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client
{
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
            lblStreetName.Text = Resources.GetValue("StreetNumberLabel");
            btSearch.TitleLabel.Text = Resources.GetValue("StreetNumberSearchBt");

            var button = new MonoTouch.UIKit.UIBarButtonItem(Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
                ViewModel.SaveCommand.Execute();
            });
            NavigationItem.RightBarButtonItem = button;

            this.AddBindings(new Dictionary<object, string>() {
                { btSearch, "{'TouchUpInside':{'Path':'NavigateToSearch'}}"},             
                { txtStreetNumber, "{'Text': {'Path': 'StreetNumberOrBuildingName'}}"},
                { lblStreetName, "{'Text': {'Path': 'Model.Street'}}"}
            });
            
            ViewModel.OnViewLoaded();
            this.View.ApplyAppFont ();
        }
		
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("StreetNumberTitle"), true);
        }

    }
}

