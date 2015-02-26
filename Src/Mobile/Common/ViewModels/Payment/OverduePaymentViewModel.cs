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
		private IAccountService _accountService;

		private OverduePayment _overduePayment;

		public OverduePaymentViewModel(IAccountService accountService)
		{
			_accountService = accountService;
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

		private decimal AmountDue
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
				return this.GetCommand(() => 
				{ 
					
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

