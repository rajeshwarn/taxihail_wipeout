
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
	public partial class OverduePayementView : BaseViewController<OverduePayementViewModel>
	{
		public OverduePayementView()
			: base("OverduePayementView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

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

			FlatButtonStyle.Silver.ApplyTo(btnRetry);
			FlatButtonStyle.Silver.ApplyTo(btnAddNewCard);

			var set = this.CreateBindingSet<OverduePayementView, OverduePayementViewModel>();

			set.Bind(TransactionId)
				.To(vm => vm.TransactionNumber);

			set.Bind(DateOfTransaction)
				.To(vm => vm.DateOfTransaction);

			set.Bind(AmountDue)
				.To(vm => vm.Amount);

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

