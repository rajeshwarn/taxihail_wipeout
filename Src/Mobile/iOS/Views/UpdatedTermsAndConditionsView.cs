using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class UpdatedTermsAndConditionsView : BaseViewController<UpdatedTermsAndConditionsViewModel>
    {
        public UpdatedTermsAndConditionsView () : base("UpdatedTermsAndConditionsView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            NavigationItem.HidesBackButton = true;
            NavigationItem.Title = Localize.GetValue("View_UpdatedTermsAndConditions");
            NavigationController.NavigationBar.Hidden = false;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad();

            FlatButtonStyle.Silver.ApplyTo (btnAccept);
            btnAccept.SetTitle(Localize.GetValue("UpdatedTermsAndConditionsAcceptButton"), UIControlState.Normal);

            var set = this.CreateBindingSet<UpdatedTermsAndConditionsView, UpdatedTermsAndConditionsViewModel> ();

            set.BindSafe(txtTermsAndConditions)
                .For(v => v.Text)
                .To(vm => vm.TermsAndConditions);

            set.Bind(btnAccept)
                .For("TouchUpInside")
                .To(vm => vm.Confirm);

            set.Apply ();
        }
    }
}
