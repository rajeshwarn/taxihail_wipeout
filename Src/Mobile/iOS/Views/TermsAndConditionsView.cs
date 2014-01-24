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
	public partial class TermsAndConditionsView : BaseViewController<TermsAndConditionsViewModel>
    {
        public TermsAndConditionsView () : base("TermsAndConditionsView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            ApplyThemeToNavigationBar();

            base.ViewWillAppear (animated);
            NavigationItem.HidesBackButton = false;
            NavigationItem.Title = Localize.GetValue("TermsAndConditionsLabel");
            NavigationController.NavigationBar.Hidden = false;
            NavigationController.NavigationBar.BarStyle = Theme.IsLightContent 
                                                          ? UIBarStyle.Black 
                                                          : UIBarStyle.Default;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad();

			var set = this.CreateBindingSet<TermsAndConditionsView, TermsAndConditionsViewModel> ();

			set.BindSafe(txtTermsAndConditions)
				.For(v => v.Text)
				.To(vm => vm.TermsAndConditions);

			set.Apply ();
        }
    }
}
