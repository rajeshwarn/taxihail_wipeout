
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualRideLinqSummaryView : BaseViewController<ManualRideLinqStatusViewModel>
	{
		public ManualRideLinqSummaryView()
			: base("ManualRideLinqSummaryView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var localize = this.Services().Localize;

			NavigationController.NavigationBar.Hidden = false;

			NavigationItem.Title = localize["View_RideLinqStatus"];
			lblPairingCodeLabel.Text = localize["ManualRideLinqStatus_PairingCode"];
			lblMedallionLabel.Text = localize["ManualRideLinqStatus_Medallion"];
			lblDistanceLabel.Text = localize["ManualRideLinqStatus_Distance"];
			lblTotalLabel.Text = localize["ManualRideLinqStatus_Total"];
			lblFareLabel.Text = localize["ManualRideLinqStatus_Fare"];
			lblTaxLabel.Text = localize["ManualRideLinqStatus_Tax"];
			lblTipLabel.Text = localize["ManualRideLinqStatus_Tip"];
			btnUnpair.SetTitle(localize["ManualRideLinqStatus_Unpair"], UIControlState.Normal);

			FlatButtonStyle.Red.ApplyTo(btnUnpair);

			var bindingSet = this.CreateBindingSet<ManualRideLinqSummaryView, ManualRideLinqStatusViewModel>();

			bindingSet.Bind(lblPairingCodeText)
				.To(vm => vm.OrderManualRideLinqDetail.PairingCode);

			bindingSet.Bind(lblMedallionText)
				.To(vm => vm.OrderManualRideLinqDetail.Medallion);

			bindingSet.Bind(lblDistanceText)
				.To(vm => vm.OrderManualRideLinqDetail.Distance);

			bindingSet.Bind(lblFareText)
				.To(vm => vm.OrderManualRideLinqDetail.Fare);

			bindingSet.Bind(lblTaxText)
				.To(vm => vm.OrderManualRideLinqDetail.Tax);

			bindingSet.Bind(lblTipText)
				.To(vm => vm.OrderManualRideLinqDetail.Tip);

			bindingSet.Bind(btnUnpair)
				.For(v => v.Command)
				.To(vm => vm.UnpairFromRideLinq);

			bindingSet.Bind(lblTotalText)
				.To(vm => vm.OrderManualRideLinqDetail.Total);
			
			bindingSet.Apply();
		}
	}
}

