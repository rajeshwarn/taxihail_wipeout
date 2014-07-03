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
			NavigationItem.HidesBackButton = false;
            NavigationItem.Title = Localize.GetValue("RideSummaryTitleText");

            ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			FlatButtonStyle.Green.ApplyTo(btnReSendConfirmation);
			FlatButtonStyle.Green.ApplyTo(btnPay);

            lblSubTitle.Text = String.Format(Localize.GetValue ("RideSummarySubTitleText"), this.Services().Settings.ApplicationName);

            btnPay.SetTitle(Localize.GetValue("PayNow"), UIControlState.Normal);
            btnReSendConfirmation.SetTitle(Localize.GetValue("ReSendConfirmation"), UIControlState.Normal);

            // configure tableview for ratings
            var btnSubmit = new FlatButton(new RectangleF(8f, BottomPadding, 304f, 41f));
            FlatButtonStyle.Green.ApplyTo(btnSubmit);
            btnSubmit.SetTitle(Localize.GetValue("Submit"), UIControlState.Normal);

            var footerView = new UIView(new RectangleF(0f, 0f, 320f, btnSubmit.Frame.Height + BottomPadding * 2));
            footerView.AddSubview(btnSubmit);
            tableRatingList.TableFooterView = footerView;

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

            set.Bind(btnPay)
				.For("TouchUpInside")
				.To(vm => vm.PayCommand);
            set.Bind(btnPay)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsPayButtonShown)
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
                        constraintRatingTableHeight.Constant = BookRatingCell.Height * ViewModel.RatingList.Count + footerView.Frame.Height;
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
            base.ViewWillDisappear(animated);
            if (IsMovingFromParentViewController)
            {
                // Back button pressed
				ViewModel.PrepareNewOrder.Execute(null);
            }
        }
	}
}

