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
                    var localize = this.Services().Localize;

				    using (this.Services().Message.ShowProgress())
				    {
                        var overduePaymentResult = await _paymentService.SettleOverduePayment();

                        if (overduePaymentResult.IsSuccessful)
                        {
                            var message = string.Format(this.Services().Localize["Overdue_Succeed_Message"], _overduePayment.OverdueAmount);

                            await this.Services().Message.ShowMessage(localize["Overdue_Succeed_Title"], message);

                            Close(this);
                        }
                        else
                        {
                            await this.Services().Message.ShowMessage(localize["Overdue_Failed_Title"], localize["Overdue_Failed_Message"]);
                        }
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

