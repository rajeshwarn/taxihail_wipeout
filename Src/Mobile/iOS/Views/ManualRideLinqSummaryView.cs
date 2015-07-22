using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Order;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualRideLinqSummaryView : BaseViewController<ManualRideLinqSummaryViewModel>
	{
        private MvxActionBasedTableViewSource _source;

		public ManualRideLinqSummaryView()
			: base("ManualRideLinqSummaryView", null)
		{
		}

	    public override void ViewWillAppear(bool animated)
	    {
	        base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.HidesBackButton = true;

            ChangeThemeOfBarStyle();
	    }

	    public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var localize = this.Services().Localize;

			NavigationItem.Title = localize["RideSummaryTitleText"];
			lblDistanceLabel.Text = localize["ManualRideLinqStatus_Distance"];
			lblTotalLabel.Text = localize["ManualRideLinqStatus_Total"];
			lblFareLabel.Text = localize["ManualRideLinqStatus_Fare"];
			lblTaxLabel.Text = localize["ManualRideLinqStatus_Tax"];
			lblTipLabel.Text = localize["ManualRideLinqStatus_Tip"];
			lblExtraLabel.Text = localize["ManualRideLinqStatus_Extra"];
			lblTollLabel.Text = localize["ManualRideLinqStatus_Toll"];

			lblThanks.Text = String.Format(localize["RideSummarySubTitleText"], this.Services().Settings.TaxiHail.ApplicationName);

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

            PrepareTableView();

            ViewModel.PropertyChanged += (sender, e) =>
                {
                    if(e.PropertyName == "RatingList")
                    {
                        ResizeTableView();
                    }
                };
            
			var bindingSet = this.CreateBindingSet<ManualRideLinqSummaryView, ManualRideLinqSummaryViewModel>();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(localize["ManualRideLinqSummary_Done"], UIBarButtonItemStyle.Bordered, (o, e) => 
                {  
                    ViewModel.RateOrderAndNavigateToHome.ExecuteIfPossible();
                });

            bindingSet.Bind(_source)
                .For(v => v.ItemsSource)
                .To(vm => vm.RatingList);

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

			bindingSet.Bind(lblTollText)
				.To(vm => vm.FormattedToll);
			
			bindingSet.Bind(lblExtraText)
				.To(vm => vm.FormattedExtra);

			bindingSet.Bind(lblTotalText)
				.To(vm => vm.FormattedTotal);
			
			bindingSet.Apply();
		}

        private void PrepareTableView()
        {
            _source = new MvxActionBasedTableViewSource(
                tableRatingList,
                UITableViewCellStyle.Default,
                BookRatingCell.Identifier,
                BookRatingCell.BindingText,
                UITableViewCellAccessory.None);

            _source.CellCreator = (tableView, indexPath, item) =>
                {
                    var cell = BookRatingCell.LoadFromNib(tableView);
                    cell.RemoveDelay();
                    return cell;
                };

            tableRatingList.Source = _source;
        }

        private void ResizeTableView()
        {
            if (ViewModel.RatingList != null)
            {
                constraintRatingTableHeight.Constant = BookRatingCell.Height * ViewModel.RatingList.Count;
            }
            else
            {
                constraintRatingTableHeight.Constant = 0;
            }
        }
	}
}

