using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class TermsAndConditionsView : BaseViewController<TermsAndConditionsViewModel>
    {


        public TermsAndConditionsView () 
            : base(new MvxShowViewModelRequest<TermsAndConditionsViewModel>( null, true, new MvxRequestedBy()   ) )
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

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;
        }

        public override void ViewDidLoad ()
        {

            base.ViewDidLoad();
//            ViewModel.Load();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = true;

            AppButtons.FormatStandardButton((GradientButton)btnAccept, Resources.GetValue("AcceptButton"), AppStyle.ButtonColor.Grey );          
            AppButtons.FormatStandardButton((GradientButton)btnCancel, Resources.CancelBoutton, AppStyle.ButtonColor.Grey );          

            new [] { 
                lblTitle
            }
            .Where(x => x != null)
                .ForEach(x => x.TextColor = AppStyle.DarkText)
                    .ForEach(x => x.Font = AppStyle.GetBoldFont(x.Font.PointSize));

            txtTermsAndConditions.TextColor = AppStyle.LightGreyText;
            txtTermsAndConditions.Font = AppStyle.GetNormalFont(txtTermsAndConditions.Font.PointSize);

            lblAcknowledgment.TextColor = AppStyle.GreyText;
            lblAcknowledgment.Font = AppStyle.GetNormalFont(txtTermsAndConditions.Font.PointSize);

            lblTitle.Text = Resources.GetValue("TermsAndConditionsLabel");
            lblAcknowledgment.Text = Resources.GetValue("TermsAndConditionsAcknowledgment");

            var bindings = new [] {
                Tuple.Create<object,string>(btnAccept, "{'TouchUpInside':{'Path':'AcceptTermsAndConditions'}}"),
                Tuple.Create<object,string>(btnCancel, "{'TouchUpInside':{'Path':'RejectTermsAndConditions'}}"),
                Tuple.Create<object,string>(txtTermsAndConditions, "{'Text': {'Path': 'TermsAndConditions'}}")
            }
            .Where(x=> x.Item1 != null )
                .ToDictionary(x=>x.Item1, x=>x.Item2);

            this.AddBindings(bindings);

            View.ApplyAppFont ();
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            NavigationItem.TitleView = new TitleView (null, string.Empty, false);

        }
    }
}
