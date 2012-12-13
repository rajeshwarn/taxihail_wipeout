
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class CreditCardsListView :  BaseViewController<CreditCardsListViewModel>
    {
        #region Constructors
        
        public CreditCardsListView () 
            : base(new MvxShowViewModelRequest<CreditCardsListViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public CreditCardsListView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public CreditCardsListView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
        
        #endregion
		
        	
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
			
            View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

            NavigationItem.HidesBackButton = false;
            var add = new UIBarButtonItem(UIBarButtonSystemItem.Add, null, null);
            add.Clicked += (sender, e) => ViewModel.NavigateToAddCreditCard.Execute();
            NavigationItem.RightBarButtonItem = add;

            //this.AddBindings(new Dictionary<object, string>(){ });
            NavigationItem.Title = Resources.GetValue( "CreditCardsListTitle");
            this.View.ApplyAppFont ();
        }	

    }
}

