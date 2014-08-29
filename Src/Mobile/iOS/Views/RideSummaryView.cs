using System;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Order;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RideSummaryView  : BaseViewController<RideSummaryViewModel>
	{     
		public RideSummaryView() : base("RideSummaryView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.HidesBackButton = true;
            NavigationItem.Title = Localize.GetValue("RideSummaryTitleText");

            ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			FlatButtonStyle.Green.ApplyTo(btnReSendConfirmation);
			FlatButtonStyle.Green.ApplyTo(btnPay);
            FlatButtonStyle.Silver.ApplyTo(btnSubmit);

            lblSubTitle.Text = String.Format(Localize.GetValue ("RideSummarySubTitleText"), this.Services().Settings.ApplicationName);

            btnPay.SetTitle(Localize.GetValue("PayNow"), UIControlState.Normal);
            btnReSendConfirmation.SetTitle(Localize.GetValue("ReSendConfirmation"), UIControlState.Normal);
            btnSubmit.SetTitle(Localize.GetValue("Submit"), UIControlState.Normal);

            var source = new MvxActionBasedTableViewSource(
                tableRatingList,
                UITableViewCellStyle.Default,
                BookRatingCell.Identifier ,
                BookRatingCell.BindingText,
                UITableViewCellAccessory.None);

            source.CellCreator = (tableView, indexPath, item) =>
            {
                var cell = BookRatingCell.LoadFromNib(tableView);
                cell.RemoveDelay();
                return cell;
            };

            tableRatingList.Source = source;

			var set = this.CreateBindingSet<RideSummaryView, RideSummaryViewModel> ();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("Done"), UIBarButtonItemStyle.Bordered, (o, e) => 
            {  
                if (ViewModel.CanUserLeaveScreen ())
                {
					ViewModel.CloseCommand.Execute(null);                    
                }
            });

            set.Bind(btnPay)
				.For("TouchUpInside")
				.To(vm => vm.PayCommand);
            set.Bind(btnPay)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsPayButtonShown)
                .WithConversion("BoolInverter");

            set.Bind (btnSubmit)
                .For ("TouchUpInside")
                .To (vm => vm.RateOrder);
            set.Bind (btnSubmit)
                .For (v => v.HiddenWithConstraints)
                .To (vm => vm.IsRatingButtonShown)
                .WithConversion("BoolInverter");

            set.Bind(btnReSendConfirmation)
				.For("TouchUpInside")
				.To(vm => vm.ResendConfirmationCommand);
            set.Bind(btnReSendConfirmation)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsResendConfirmationButtonShown)
                .WithConversion("BoolInverter");

            set.Bind(source)
                .For(v => v.ItemsSource)
                .To(vm => vm.RatingList);

			set.Apply ();

            ViewModel.PropertyChanged += (sender, e) =>
            {
                if(e.PropertyName == "RatingList")
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
            };
		}

        public override void ViewWillDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
            {
                // Back button pressed
				ViewModel.PrepareNewOrder.Execute(null);
            }

            base.ViewWillDisappear(animated);
        }
	}
}

