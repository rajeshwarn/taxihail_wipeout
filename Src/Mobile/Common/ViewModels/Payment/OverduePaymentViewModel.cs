using System;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class OverduePaymentViewModel : PageViewModel
	{
		private IPaymentService _paymentService;

		private OverduePayment _overduePayment;

		public OverduePaymentViewModel(IPaymentService accountService)
		{
			_paymentService = accountService;
		}

		public void Init(string overduePayement)
		{
			OverduePayment = JsonSerializer.DeserializeFromString<OverduePayment>(overduePayement);
		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);

		}

		public OverduePayment OverduePayment
		{
			get
			{
				return _overduePayment;
			}
			set
			{
				_overduePayment = value;

				RaisePropertyChanged();
				RaisePropertyChanged(() => AmountDue);
			}
		}

		public decimal AmountDue
		{
			get
			{
				return _overduePayment != null
					? _overduePayment.OverdueAmount
					: 0;
			}
		}

		public ICommand Retry
		{
			get
			{
				return this.GetCommand(async () => 
				{ 
					var overduePaymentResult = await _paymentService.SettleOverduePayment();
					
					if(overduePaymentResult.IsSuccessful)
					{
						ShowViewModelAndRemoveFromHistory<CreditCardAddViewModel>(null);
					}
					else
					{
						var title = "Error settling overdue payment.";
						var message = "Your credit transaction was denied, please try again later or replace your credit card with a new one.";
						await this.Services().Message.ShowMessage(title, message);
					}
				});
			}
		}

		public ICommand AddNewCard
		{
			get
			{
				return this.GetCommand(() => 
				{
					ShowViewModel<CreditCardAddViewModel>(new { showInstructions = true });
				});
			}
		}
	}
}

