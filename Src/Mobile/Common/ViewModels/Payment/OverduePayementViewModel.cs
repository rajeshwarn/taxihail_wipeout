using System;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class OverduePayementViewModel : PageViewModel
	{
		private IAccountService _accountService;

		public OverduePayementViewModel(IAccountService accountService)
		{
			_accountService = accountService;

		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);

		}
			
		public string TransactionNumber
		{
			get { return "AVX1212121"; }
			set { }
		}

		public DateTime DateOfTransaction
		{
			get;
			set;
		}

		public double Amount
		{
			get { return 0.0;}
			set { }
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

