using System;
using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.BindingConverter;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class HistoryDetailView : MvxViewController
    {
		public HistoryDetailView() 
			: base("HistoryDetailView", null)
		{
		}

		public new HistoryDetailViewModel ViewModel
		{
			get
			{
				return (HistoryDetailViewModel)DataContext;
			}
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            
            NavigationItem.HidesBackButton = false;
            NavigationItem.TitleView = new TitleView(null, Localize.GetValue("View_HistoryDetail"), true);

            lblConfirmationNo.Text = Localize.GetValue("HistoryDetailConfirmationLabel");
            lblRequested.Text = Localize.GetValue("HistoryDetailRequestedLabel");
            lblOrigin.Text = Localize.GetValue("HistoryDetailOriginLabel");
            lblAptRingCode.Text = Localize.GetValue("HistoryDetailAptRingCodeLabel");
            lblDestination.Text = Localize.GetValue("HistoryDetailDestinationLabel");
			lblDestination.Hidden = ViewModel.HideDestination;
            lblPickupDate.Text = Localize.GetValue("HistoryDetailPickupDateLabel");
            lblStatus.Text = Localize.GetValue("HistoryDetailStatusLabel");
            lblAuthorization.Text = Localize.GetValue("HistoryDetailAuthorizationLabel");

            btnRebook.SetTitle(Localize.GetValue("HistoryDetailRebookButton"), UIControlState.Normal);
            btnStatus.SetTitle(Localize.GetValue("HistoryViewStatusButton"), UIControlState.Normal);
            btnSendReceipt.SetTitle(Localize.GetValue("HistoryViewSendReceiptButton"), UIControlState.Normal);
            btnRateTrip.SetTitle(Localize.GetValue("RateBtn"), UIControlState.Normal);
            btnViewRating.SetTitle(Localize.GetValue("ViewRatingBtn"), UIControlState.Normal);
            AppButtons.FormatStandardButton((GradientButton)btnHide, Localize.GetValue("DeleteButton"), AppStyle.ButtonColor.Red);
            AppButtons.FormatStandardButton((GradientButton)btnCancel, Localize.GetValue("StatusActionCancelButton"), AppStyle.ButtonColor.Red);

			var set = this.CreateBindingSet<HistoryDetailView, HistoryDetailViewModel>();

			set.Bind(btnRebook)
				.For(v => v.Hidden)
				.To(vm => vm.RebookIsAvailable)
				.WithConversion("BoolInverter");
			set.Bind(btnRebook)
				.For("TouchUpInside")
				.To(vm => vm.RebookOrder);

			set.Bind(btnHide)
				.For(v => v.Hidden)
				.To(vm => vm.IsCompleted)
				.WithConversion("BoolInverter");
			set.Bind(btnHide)
				.For("TouchUpInside")
				.To(vm => vm.DeleteOrder);

			set.Bind(btnStatus)
				.For(v => v.Hidden)
				.To(vm => vm.IsCompleted);
			set.Bind(btnStatus)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToOrderStatus);

			set.Bind(btnCancel)
				.For(v => v.Hidden)
				.To(vm => vm.IsCompleted);
			set.Bind(btnCancel)
				.For("TouchUpInside")
				.To(vm => vm.CancelOrder);

			set.Bind(btnRateTrip)
				.For(v => v.Hidden)
				.To(vm => vm.ShowRateButton)
				.WithConversion("BoolInverter");
			set.Bind(btnRateTrip)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToRatingPage);

			set.Bind(btnViewRating)
				.For(v => v.Hidden)
				.To(vm => vm.HasRated)
				.WithConversion("BoolInverter");
			set.Bind(btnViewRating)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToRatingPage);

			set.Bind(btnSendReceipt)
				.For(v => v.Hidden)
				.To(vm => vm.SendReceiptAvailable)
				.WithConversion("BoolInverter");
			set.Bind(btnSendReceipt)
				.For("TouchUpInside")
				.To(vm => vm.SendReceipt);

			set.Bind(txtConfirmationNo)
				.For(v => v.Text)
				.To(vm => vm.ConfirmationTxt);

			set.Bind(txtDestination)
				.For(v => v.Text)
				.To(vm => vm.DestinationTxt);
			set.Bind(txtDestination)
				.For(v => v.Hidden)
				.To(vm => vm.HideDestination);

			set.Bind(txtOrigin)
				.For(v => v.Text)
				.To(vm => vm.OriginTxt);

			set.Bind(txtRequested)
				.For(v => v.Text)
				.To(vm => vm.RequestedTxt);

			set.Bind(txtAptCode)
				.For(v => v.Text)
				.To(vm => vm.AptRingTxt);

			set.Bind(txtStatus)
				.For(v => v.Text)
				.To(vm => vm.Status.IbsStatusDescription);

			set.Bind(txtPickupDate)
				.For(v => v.Text)
				.To(vm => vm.PickUpDateTxt);

			set.Bind(lblAuthorization)
				.For(v => v.Hidden)
				.To(vm => vm.AuthorizationNumber)
				.WithConversion("NoValueToTrueConverter");

			set.Bind(txtAthorization)
				.For(v => v.Text)
				.To(vm => vm.AuthorizationNumber);

			set.Apply ();

			ViewModel.Loaded+= (sender, e) => {
				InvokeOnMainThread(()=>{
					ReorderButtons();
					ReorderLabels();
				});
			};

            ViewModel.Load();

            View.ApplyAppFont ();
        }

		void ReorderButtons()
		{
			var yPositionOfFirstButton = btnRebook.Frame.Y;
			var deltaYBetweenButtons = btnHide.Frame.Y - yPositionOfFirstButton;

			var buttonList = new List<UIButton> ();
			buttonList.Add (btnRebook);
			buttonList.Add (btnStatus);
			buttonList.Add (btnRateTrip);
			buttonList.Add (btnViewRating);
			buttonList.Add (btnSendReceipt);
			buttonList.Add (btnHide);
			buttonList.Add (btnCancel);

			var i = 0;
			foreach (var item in buttonList) {
				if (!item.Hidden) {
					
					item.Frame = new RectangleF(item.Frame.X, yPositionOfFirstButton + (deltaYBetweenButtons * i), item.Frame.Width, item.Frame.Height);
					i++;
				}
			}
		}

		void ReorderLabels()
		{
			var yPositionOfFirstLabel = lblConfirmationNo.Frame.Y;
			var deltaYBetweenLabels = lblRequested.Frame.Y - yPositionOfFirstLabel;

			var labelList = new List<Tuple<UILabel,UILabel>> ();
			labelList.Add (new Tuple<UILabel, UILabel>(lblConfirmationNo, txtConfirmationNo));
			labelList.Add (new Tuple<UILabel, UILabel>(lblRequested, txtRequested));
			labelList.Add (new Tuple<UILabel, UILabel>(lblOrigin, txtOrigin));
			labelList.Add (new Tuple<UILabel, UILabel>(lblAptRingCode, txtAptCode));
			labelList.Add (new Tuple<UILabel, UILabel>(lblDestination, txtDestination));
			labelList.Add (new Tuple<UILabel, UILabel>(lblPickupDate, txtPickupDate));
			labelList.Add (new Tuple<UILabel, UILabel>(lblStatus, txtStatus));
			labelList.Add (new Tuple<UILabel, UILabel>(lblAuthorization, txtAthorization));

			var i = 0;
			foreach (var item in labelList) {
				if (!item.Item1.Hidden) {
					item.Item1.SetY(yPositionOfFirstLabel + (deltaYBetweenLabels * i));
					item.Item2.SetY(yPositionOfFirstLabel + (deltaYBetweenLabels * i));
					i++;
				}
			}
		}
    }
}
