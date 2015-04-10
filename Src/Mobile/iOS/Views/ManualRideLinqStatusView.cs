
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualRideLinqStatusView : BaseViewController<ManualRideLinqStatusViewModel>
	{
		public ManualRideLinqStatusView()
			: base("ManualRideLinqStatusView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var localize = this.Services().Localize;

			NavigationController.NavigationBar.Hidden = false;

			NavigationItem.Title = localize["View_RideLinqStatus"];

			lblDriverId.Text = localize["ManualRideLinqStatus_Driver"];
			lblPairingCode.Text = localize["ManualRideLinqStatus_PairingCode"];
			lblVehicule.Text = localize["ManualRideLinqStatus_Medallion"];
			btnUnpair.SetTitle(localize["ManualRideLinqStatus_Unpair"], UIControlState.Normal);

			FlatButtonStyle.Red.ApplyTo(btnUnpair);

			var bindingSet = this.CreateBindingSet<ManualRideLinqStatusView, ManualRideLinqStatusViewModel>();

			bindingSet.Bind(lblDriverIdText)
				.To(vm => vm.OrderManualRideLinqDetail.DriverId);

			bindingSet.Bind(lblPairingCodeText)
				.To(vm => vm.OrderManualRideLinqDetail.PairingCode);

			bindingSet.Bind(lblVehiculeText)
				.To(vm => vm.OrderManualRideLinqDetail.Medallion);

			bindingSet.Bind(btnUnpair)
				.For(v => v.Command)
				.To(vm => vm.UnpairFromRideLinq);

			bindingSet.Apply();
		}
	}
}

