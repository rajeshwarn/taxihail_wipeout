
using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Foundation;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

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

			lblTransactionId.Text = localize["Overdue_TransactionId"];
			lblDate.Text = localize["Overdue_Date"];
			lblAmountDue.Text = localize["Overdue_Amount"];
			btnRetry.SetTitle(localize["Overdue_Retry"], UIControlState.Normal);
			btnAddNewCard.SetTitle(localize["Overdue_Amount"], UIControlState.Normal);

			var bindingSet = this.CreateBindingSet<OverduePayementView, OverduePayementViewModel>();

			bindingSet.Bind(TransactionId)
				.To(vm => vm.TransactionNumber);

			bindingSet.Bind(DateOfTransaction)
				.To(vm => vm.DateOfTransaction);

			bindingSet.Bind(AmountDue)
				.To(vm => vm.Amount);

			bindingSet.Bind(btnRetry)
				.For("TouchDown")
				.To(vm => vm.Retry);

			bindingSet.Bind(btnAddNewCard)
				.For("TouchDown")
				.To(vm => vm.AddNewCard);

			bindingSet.Apply();

		}
	}
}

