
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualRideLinqSummaryView : BaseViewController<ManualRideLinqSummaryViewModel>
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

			NavigationItem.Title = localize["View_RideLinqSummary"];
			lblPairingCodeLabel.Text = localize["ManualRideLinqStatus_PairingCode"];
			lblDistanceLabel.Text = localize["ManualRideLinqStatus_Distance"];
			lblTotalLabel.Text = localize["ManualRideLinqStatus_Total"];
			lblFareLabel.Text = localize["ManualRideLinqStatus_Fare"];
			lblTaxLabel.Text = localize["ManualRideLinqStatus_Tax"];
			lblTipLabel.Text = localize["ManualRideLinqStatus_Tip"];
			lblDriverId.Text = localize["ManualRideLinqStatus_Driver"];

			NavigationItem.RightBarButtonItem = new UIBarButtonItem()
				{
					Title = localize["ManualRideLinqSummary_Done"]
				};

			var bindingSet = this.CreateBindingSet<ManualRideLinqSummaryView, ManualRideLinqSummaryViewModel>();

			bindingSet.Bind(lblPairingCodeText)
				.To(vm => vm.OrderManualRideLinqDetail.PairingCode);

			bindingSet.Bind(lblDriverIdText)
				.To(vm => vm.OrderManualRideLinqDetail.DriverId);

			bindingSet.Bind(lblDistanceText)
				.To(vm => vm.OrderManualRideLinqDetail.Distance);

			bindingSet.Bind(lblFareText)
				.To(vm => vm.OrderManualRideLinqDetail.Fare);

			bindingSet.Bind(lblTaxText)
				.To(vm => vm.OrderManualRideLinqDetail.Tax);

			bindingSet.Bind(lblTipText)
				.To(vm => vm.OrderManualRideLinqDetail.Tip);

			bindingSet.Bind(lblTotalText)
				.To(vm => vm.OrderManualRideLinqDetail.Total);

			bindingSet.Bind(NavigationItem.RightBarButtonItem)
				.To(vm => vm.GoToHome);
			
			bindingSet.Apply();
		}
	}
}

