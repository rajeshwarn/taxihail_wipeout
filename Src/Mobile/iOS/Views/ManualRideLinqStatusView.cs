
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualRideLinqStatusView : BaseViewController<ManualRideLinqStatusViewModel>
	{
		public ManualRideLinqStatusView()
			: base("ManualRideLinqStatusView", null)
		{
		}


	    public override void ViewWillAppear(bool animated)
	    {
	        base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;

			ChangeThemeOfBarStyle();
	    }

	    public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var localize = this.Services().Localize;

			NavigationItem.Title = localize["View_RideLinqStatus"];
			lblGreetings.Text = localize["ManualRideLinqStatus_Greetings"];

			btnTip.SetTitle(localize ["StatusEditAutoTipButton"], UIControlState.Normal);

			FlatButtonStyle.Silver.ApplyTo(btnTip);
			FlatButtonStyle.Red.ApplyTo(btnUnpair);

			var bindingSet = this.CreateBindingSet<ManualRideLinqStatusView, ManualRideLinqStatusViewModel>();

			bindingSet.Bind(lblMedallion)
				.To(vm => vm.Medallion)
				.WithConversion("StringFormat", localize["ManualRideLinqStatus_Medallion"]);

			bindingSet.Bind(lblPayment)
				.To(vm => vm.PaymentInfo);

			bindingSet.Bind(lblEmail)
				.To(vm => vm.Email)
				.WithConversion("StringFormat", localize["ManualRideLinqStatus_Email"]);

			bindingSet.Bind (btnTip)
				.For(v => v.Command)
				.To(vm => vm.EditAutoTipCommand);

			bindingSet.Apply();
		}
	}
}