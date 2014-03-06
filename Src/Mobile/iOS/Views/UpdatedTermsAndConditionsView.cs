using System;
using System.Linq;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

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

            NavigationItem.HidesBackButton = false;
            NavigationItem.Title = Localize.GetValue("View_UpdatedTermsAndConditions");
            NavigationController.NavigationBar.Hidden = false;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad();

            var set = this.CreateBindingSet<UpdatedTermsAndConditionsView, UpdatedTermsAndConditionsViewModel> ();

            set.BindSafe(txtTermsAndConditions)
                .For(v => v.Text)
                .To(vm => vm.TermsAndConditions);

            set.Apply ();
        }
    }
}
