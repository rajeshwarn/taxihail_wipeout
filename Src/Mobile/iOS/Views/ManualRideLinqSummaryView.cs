using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualRideLinqSummaryView : BaseViewController<ManualRideLinqSummaryViewModel>
	{
		public ManualRideLinqSummaryView()
			: base("ManualRideLinqSummaryView", null)
		{
		}

	    public override void ViewWillAppear(bool animated)
	    {
	        base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
	    }

	    public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var localize = this.Services().Localize;

			NavigationItem.Title = localize["RideSummaryTitleText"];
			lblPairingCodeLabel.Text = localize["ManualRideLinqStatus_PairingCode"];
			lblDistanceLabel.Text = localize["ManualRideLinqStatus_Distance"];
			lblTotalLabel.Text = localize["ManualRideLinqStatus_Total"];
			lblFareLabel.Text = localize["ManualRideLinqStatus_Fare"];
			lblTaxLabel.Text = localize["ManualRideLinqStatus_Tax"];
			lblTipLabel.Text = localize["ManualRideLinqStatus_Tip"];
			lblDriverId.Text = localize["ManualRideLinqStatus_Driver"];

            lblThanks.Text = String.Format(localize["RideSummarySubTitleText"], this.Services().Settings.TaxiHail.ApplicationName);           

			var bindingSet = this.CreateBindingSet<ManualRideLinqSummaryView, ManualRideLinqSummaryViewModel>();

			bindingSet.Bind(lblPairingCodeText)
				.To(vm => vm.OrderManualRideLinqDetail.PairingCode);

			bindingSet.Bind(lblDriverIdText)
				.To(vm => vm.OrderManualRideLinqDetail.DriverId);

			bindingSet.Bind(lblDistanceText)
				.To(vm => vm.FormattedDistance);

			bindingSet.Bind(lblFareText)
				.To(vm => vm.FormattedFare);

			bindingSet.Bind(lblTaxText)
                .To(vm => vm.FormattedTax);

			bindingSet.Bind(lblTipText)
				.To(vm => vm.FormattedTip);

			bindingSet.Bind(lblTotalText)
				.To(vm => vm.FormattedTotal);

			bindingSet.Bind(NavigationItem.RightBarButtonItem)
				.To(vm => vm.GoToHome);
			
			bindingSet.Apply();
		}
	}
}

