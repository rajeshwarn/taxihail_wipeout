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
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

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

			NavigationItem.HidesBackButton = false;
			NavigationItem.Title = Localize.GetValue("View_HistoryDetail");
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			ViewModel.OnViewLoaded();
            
			View.BackgroundColor = UIColor.FromRGB(239, 239, 239);

			FlatButtonStyle.Red.ApplyTo(btnDelete);
			FlatButtonStyle.Green.ApplyTo(btnRebook);

			lblOrder.Text = Localize.GetValue("HistoryDetailOrderLabel");
			lblRequested.Text = Localize.GetValue("HistoryDetailRequestedLabel");
			lblPickup.Text = Localize.GetValue("HistoryDetailPickupLabel");
			lblAptRingCode.Text = Localize.GetValue("HistoryDetailAptRingCodeLabel");
			txtAptRingCode.Text = Localize.GetValue("NoAptText") + " / " + Localize.GetValue("NoRingCodeText");
			lblDestination.Text = Localize.GetValue("HistoryDetailDestinationLabel");
			txtDestination.Text = Localize.GetValue("DestinationNotSpecifiedText");
			lblPickupDate.Text = Localize.GetValue("HistoryDetailPickupDateLabel");
			lblStatus.Text = Localize.GetValue("HistoryDetailStatusLabel");
			txtStatus.Text = Localize.GetValue("LoadingText");

			btnRebook.SetTitle(Localize.GetValue("Rebook"), UIControlState.Normal);
			btnDelete.SetTitle(Localize.GetValue("Delete"), UIControlState.Normal);

			if (ViewModel.HideDestination)
			{
				lblDestination.RemoveFromSuperview();
				txtDestination.RemoveFromSuperview();
			}

			var set = this.CreateBindingSet<HistoryDetailView, HistoryDetailViewModel>();

			set.Bind(btnRebook)
				.For(v => v.Hidden)
				.To(vm => vm.RebookIsAvailable)
				.WithConversion("BoolInverter");
			set.Bind(btnRebook)
				.For("TouchUpInside")
				.To(vm => vm.RebookOrder);

			set.Bind(btnDelete)
				.For(v => v.Hidden)
				.To(vm => vm.IsCompleted);
			set.Bind(btnDelete)
				.For("TouchUpInside")
				.To(vm => vm.CancelOrder);

			set.Bind(txtOrder)
				.For(v => v.Text)
				.To(vm => vm.ConfirmationTxt);

			set.BindSafe(txtDestination)
				    .For(v => v.Text)
				    .To(vm => vm.DestinationTxt);
			set.BindSafe(txtDestination)
				    .For(v => v.Hidden)
				    .To(vm => vm.HideDestination);

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
				.To(vm => vm.Status.IbsStatusDescription);

			set.Bind(txtPickupDate)
				.For(v => v.Text)
				.To(vm => vm.PickUpDateTxt);

			set.Apply();
		}
	}
}
