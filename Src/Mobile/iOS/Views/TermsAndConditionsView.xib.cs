using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Text.RegularExpressions;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using apcurium.MK.Common;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class TermsAndConditionsView : BaseViewController<TermsAndConditionsViewModel>
    {
       

		public TermsAndConditionsView () 
            : base(new MvxShowViewModelRequest<BookConfirmationViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
		public TermsAndConditionsView (MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
		public TermsAndConditionsView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }

        public override void LoadView()
        {
            base.LoadView();
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;


        }

        public override void ViewDidLoad ()
        {
		
            base.ViewDidLoad();
            ViewModel.Load();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = true;
            
            AppButtons.FormatStandardButton((GradientButton)btnAccept, Resources.GetValue("AcceptButton"), AppStyle.ButtonColor.Grey );          
            AppButtons.FormatStandardButton((GradientButton)btnCancel, Resources.CancelBoutton, AppStyle.ButtonColor.Grey );          

            lblTitle.Text = Resources.GetValue("TermsAndConditionsLabel");
            lblAcknowledgement.Text = Resources.GetValue("TermsAndConditionsAcknowledgment");

            var bindings = new [] {
				Tuple.Create<object,string>(btnAccept, "{'TouchUpInside':{'Path':'AcceptTermsAndConditions'}}"),
				Tuple.Create<object,string>(btnCancel, "{'TouchUpInside':{'Path':'RejectTermsAndConditions'}}"),
				Tuple.Create<object,string>(txtTermsAndConditions, "{'Text': {'Path': 'TermsAndConditions'}}"),
            }
                .Where(x=> x.Item1 != null )
                .ToDictionary(x=>x.Item1, x=>x.Item2);
        
            this.AddBindings(bindings);
            
            this.View.ApplyAppFont ();
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            this.NavigationItem.TitleView = new TitleView (null, string.Empty, true);
          
        }
    }
}

