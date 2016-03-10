using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class HistoryDetailView : BaseViewController<HistoryDetailViewModel>
	{
		public HistoryDetailView() 
			: base("HistoryDetailView", null)
		{
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.HidesBackButton = false;
			NavigationItem.Title = Localize.GetValue("View_HistoryDetail");

			ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            
            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			FlatButtonStyle.Green.ApplyTo(btnRebook);
			FlatButtonStyle.Silver.ApplyTo(btnStatus);

            FlatButtonStyle.Silver.ApplyTo(btnRateRide);
			FlatButtonStyle.Silver.ApplyTo(btnViewRating);

			FlatButtonStyle.Green.ApplyTo(btnSendReceipt);

			FlatButtonStyle.Red.ApplyTo(btnDelete);
			FlatButtonStyle.Red.ApplyTo(btnCancel);

			lblOrder.Text = Localize.GetValue("HistoryDetailOrderLabel");
			lblRequested.Text = Localize.GetValue("HistoryDetailRequestedLabel");
			lblPickup.Text = Localize.GetValue("HistoryDetailPickupLabel");
			lblAptRingCode.Text = Localize.GetValue("HistoryDetailAptRingCodeLabel");
			txtAptRingCode.Text = Localize.GetValue("NoAptText") + " / " + Localize.GetValue("NoRingCodeText");
			lblDestination.Text = Localize.GetValue("HistoryDetailDestinationLabel");
			txtDestination.Text = Localize.GetValue("DestinationNotSpecifiedText");
			lblPickupDate.Text = Localize.GetValue("HistoryDetailPickupDateLabel");
			lblStatus.Text = Localize.GetValue("HistoryDetailStatusLabel");
            txtStatus.Text = Localize.GetValue("LoadingMessage");
			lblAuthorization.Text = Localize.GetValue("HistoryDetailAuthorizationLabel");
            lblPromo.Text = Localize.GetValue("HistoryDetailPromoLabel");

			btnRebook.SetTitle(Localize.GetValue("Rebook"), UIControlState.Normal);
			btnStatus.SetTitle(Localize.GetValue("HistoryViewStatusButton"), UIControlState.Normal);

			btnDelete.SetTitle(Localize.GetValue("Delete"), UIControlState.Normal);
            btnCancel.SetTitle(Localize.GetValue("StatusActionCancelButton"), UIControlState.Normal);

			btnRateRide.SetTitle(Localize.GetValue("RateRide"), UIControlState.Normal);
			btnViewRating.SetTitle(Localize.GetValue("ViewRatingBtn"), UIControlState.Normal);

			btnSendReceipt.SetTitle(Localize.GetValue("HistoryViewSendReceiptButton"), UIControlState.Normal);
            			
			var set = this.CreateBindingSet<HistoryDetailView, HistoryDetailViewModel>();

			set.Bind(btnRebook)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.RebookIsAvailable)
				.WithConversion("BoolInverter");
			set.Bind(btnRebook)
				.For("TouchUpInside")
				.To(vm => vm.RebookOrder);

			set.Bind(btnStatus)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.IsCompleted);
			set.Bind(btnStatus)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToOrderStatus);

			set.Bind(btnRateRide)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.ShowRateButton)
				.WithConversion("BoolInverter");
			set.Bind(btnRateRide)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToRatingPage);

			set.Bind(btnViewRating)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.HasRated)
				.WithConversion("BoolInverter");
			set.Bind(btnViewRating)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToRatingPage);

			set.Bind(btnSendReceipt)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.SendReceiptAvailable)
				.WithConversion("BoolInverter");
			set.Bind(btnSendReceipt)
				.For("TouchUpInside")
				.To(vm => vm.SendReceipt);

			set.Bind(btnDelete)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.IsCompleted)
				.WithConversion("BoolInverter");
			set.Bind(btnDelete)
				.For("TouchUpInside")
				.To(vm => vm.DeleteOrder);

			set.Bind(btnCancel)
				.For(v => v.HiddenWithConstraints)
                .To(vm => vm.CanCancel)
                .WithConversion("BoolInverter");
			set.Bind(btnCancel)
				.For("TouchUpInside")
				.To(vm => vm.CancelOrder);

            set.Bind(lblOrder)
                .For("HiddenEx")
                .To(vm => vm.ShowConfirmationTxt)
                .WithConversion("BoolInverter");

			set.Bind(txtOrder)
				.For(v => v.Text)
				.To(vm => vm.ConfirmationTxt);
            set.Bind(txtOrder)
                .For("HiddenEx")
                .To(vm => vm.ShowConfirmationTxt)
                .WithConversion("BoolInverter");
            
            set.Bind(lblDestination)
                .For("HiddenEx")
                .To(vm => vm.Settings.HideDestination);
            
			set.BindSafe(txtDestination)
			    .For(v => v.Text)
			    .To(vm => vm.DestinationTxt);
            set.BindSafe(txtDestination)
                .For("HiddenEx")
                .To(vm => vm.Settings.HideDestination);
            
			set.Bind(txtPickup)
				.For(v => v.Text)
				.To(vm => vm.OriginTxt);

			set.Bind(txtRequested)
				.For(v => v.Text)
				.To(vm => vm.RequestedTxt);

			set.Bind(txtAptRingCode)
				.For(v => v.Text)
				.To(vm => vm.AptRingTxt);

			set.Bind(txtStatus)
				.For(v => v.Text)
				.To(vm => vm.StatusDescription);

			set.Bind(lblAuthorization)
				.For(v => v.Hidden)
				.To(vm => vm.AuthorizationNumber)
				.WithConversion("NoValueToTrueConverter");

			set.Bind(txtAthorization)
				.For(v => v.Hidden)
				.To(vm => vm.AuthorizationNumber)
				.WithConversion("NoValueToTrueConverter");
			set.Bind(txtAthorization)
				.For(v => v.Text)
				.To(vm => vm.AuthorizationNumber);

			set.Bind(txtPickupDate)
				.For(v => v.Text)
				.To(vm => vm.PickUpDateTxt);

            set.Bind(lblPromo)
                .For("HiddenEx")
                .To(vm => vm.PromoCode)
                .WithConversion("NoValueToTrueConverter");

            set.BindSafe(txtPromo)
                .For(v => v.Text)
                .To(vm => vm.PromoCode);
            set.BindSafe(txtPromo)
                .For("HiddenEx")
                .To(vm => vm.PromoCode)
                .WithConversion("NoValueToTrueConverter");

			set.Apply();
		}
	}
}
