using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class PaymentDetailsViewModel : BaseViewModel
	{
		private readonly IAccountService _accountService;

		public PaymentDetailsViewModel(IAccountService accountService)
		{
			_accountService = accountService;
		}

		private int _defaultTipPercentage;

		public async void Start(PaymentInformation paymentDetails = null)
		{
			_defaultTipPercentage = Settings.DefaultTipPercentage;

			Tips = new[]
			{ 
				new ListItem { Id = 0,  Display = "0%" }, 
				new ListItem { Id = 5,  Display = "5%" }, 
				new ListItem { Id = 10, Display = "10%" }, 
				new ListItem { Id = 15, Display = "15%" }, 
				new ListItem { Id = 18, Display = "18%" }, 
				new ListItem { Id = 20, Display = "20%" },
				new ListItem { Id = 25, Display = "25%" }
			};

			if (paymentDetails == null)
			{
				paymentDetails = new PaymentInformation();
			}

			SelectedCreditCard = await _accountService.GetCreditCard();
			if (SelectedCreditCard != null)
			{
				paymentDetails.CreditCardId = SelectedCreditCard.CreditCardId;
			}

			var currentAccount = _accountService.CurrentAccount;
			if (!paymentDetails.TipPercent.HasValue)
			{
				if (currentAccount.DefaultTipPercent.HasValue)
				{
					paymentDetails.TipPercent = currentAccount.DefaultTipPercent;
				}
				else
				{
					paymentDetails.TipPercent = _defaultTipPercentage;
				}
			}

			Tip = paymentDetails.TipPercent.Value;
		}
    
		private CreditCardDetails _selectedCreditCard;
		public CreditCardDetails SelectedCreditCard 
		{
			get { return _selectedCreditCard; }
			set
			{
				_selectedCreditCard = value;
				RaisePropertyChanged ();
				RaisePropertyChanged (() => SelectedCreditCardId);
				RaisePropertyChanged (() => HasCreditCard);
			}
		}
    
		public Guid SelectedCreditCardId 
		{
			get 
			{ 
				return SelectedCreditCard != null 
					? SelectedCreditCard.CreditCardId 
					: Guid.Empty; 
			}
		}

		public ListItem[] Tips { get; set; }

        public string CurrencySymbol 
		{
            get 
			{
				var culture = new CultureInfo(Settings.PriceFormat);
                return culture.NumberFormat.CurrencySymbol;
            }
        }

        public bool HasCreditCard
		{
            get 
			{
				return SelectedCreditCard != null;
            }
        }

		private int _tip;
        public int Tip 
        { 
            get
            {
                return _tip;
            }
            set
			{
                _tip = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => TipAmount);
            }
        }

		public string TipAmount
		{
			get
			{
				return Tips.First(x => x.Id == Tip).Display;
			}
		}

        public bool TipListDisabled = false;

        public string TipAmountDisplay
        {
            get
            {
                return TipListDisabled ? "" : Tips.First(x => x.Id == Tip).Display;
            }
        }
    }
}