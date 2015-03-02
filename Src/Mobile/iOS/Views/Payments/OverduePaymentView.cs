﻿
using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Foundation;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	public partial class OverduePaymentView : BaseViewController<OverduePaymentViewModel>
	{
		public OverduePaymentView()
			: base("OverduePaymentView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

			var localize = this.Services().Localize;

			if (NavigationController != null)
			{
				NavigationController.NavigationBar.Hidden = false;
				ChangeThemeOfBarStyle();
			}


			NavigationItem.Title = Localize.GetValue("Overdue_View");
			NavigationItem.HidesBackButton = false;

			lblTransactionId.Text = Localize.GetValue("Overdue_TransactionId");
			lblDate.Text = Localize.GetValue("Overdue_Date");
			lblAmountDue.Text = Localize.GetValue("Overdue_Amount");
			btnRetry.SetTitle(Localize.GetValue("Overdue_Retry"), UIControlState.Normal);
			btnAddNewCard.SetTitle(Localize.GetValue("Overdue_AddNewCard"), UIControlState.Normal);
			lblOrderId.Text = Localize.GetValue("Overdue_IBSOrderId");
			lblInstructions.Text = Localize.GetValue("Overdue_Instructions");

			FlatButtonStyle.Silver.ApplyTo(btnAddNewCard);
			FlatButtonStyle.Red.ApplyTo(btnRetry);

			var set = this.CreateBindingSet<OverduePaymentView, OverduePaymentViewModel>();

			set.Bind(TransactionId)
				.To(vm => vm.OverduePayment.TransactionId);

			set.Bind(DateOfTransaction)
				.To(vm => vm.OverduePayment.TransactionDate)
				.WithConversion("DateTimeFormat", "SDT");

			set.Bind(AmountDue)
				.To(vm => vm.AmountDue)
				.WithConversion("CurrencyFormat");

			set.Bind(btnRetry)
				.For(v => v.Command)
				.To(vm => vm.Retry);

			set.Bind(btnAddNewCard)
				.For(v => v.Command)
				.To(vm => vm.AddNewCard);

			set.Apply();
		}
	}
}

