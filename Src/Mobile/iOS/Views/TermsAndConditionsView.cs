using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class TermsAndConditionsView : BaseViewController<TermsAndConditionsViewModel>
    {
        public TermsAndConditionsView () : base("TermsAndConditionsView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            NavigationItem.HidesBackButton = false;
            NavigationItem.Title = Localize.GetValue("View_TermsAndConditions");
            NavigationController.NavigationBar.Hidden = false;
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
