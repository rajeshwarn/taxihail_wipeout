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

		public void Start(PaymentInformation paymentDetails = null)
		{
			CreditCards.CollectionChanged += (sender, e) => RaisePropertyChanged(() => HasCreditCards);

			_defaultTipPercentage = Settings.DefaultTipPercentage;

			LoadCreditCards();

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

			var currentAccount = _accountService.CurrentAccount;

			// check null and set to default values in case of null            
			if (!paymentDetails.CreditCardId.HasValue)
			{
				var creditCards = _accountService.GetCreditCards();
				if (currentAccount.DefaultCreditCard.HasValue 
					&& creditCards.Any(x => x.CreditCardId == currentAccount.DefaultCreditCard.Value))
				{
					paymentDetails.CreditCardId = currentAccount.DefaultCreditCard;
				}
				else
				{
					if (creditCards.Any())
					{
                        paymentDetails.CreditCardId = creditCards.First().CreditCardId;
					}
				}
			}

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

			SelectedCreditCardId = paymentDetails.CreditCardId.GetValueOrDefault();
			Tip = paymentDetails.TipPercent.Value;
		}
    
        private readonly ObservableCollection<CreditCardDetails> _creditCards = new ObservableCollection<CreditCardDetails>();
		public ObservableCollection<CreditCardDetails> CreditCards  { get { return _creditCards; } }
    
		public ListItem[] Tips { get; set; }

		private Guid _selectedCreditCardId;
        public Guid SelectedCreditCardId 
		{
			get { return _selectedCreditCardId; }
			set 
			{
                if(value != _selectedCreditCardId)
                {
                    _selectedCreditCardId = value;
					RaisePropertyChanged(()=>SelectedCreditCardId);
					RaisePropertyChanged(()=>SelectedCreditCard);
                }
            }
        }

        public CreditCardDetails SelectedCreditCard 
		{
            get
			{ 
				return CreditCards.FirstOrDefault(x => x.CreditCardId == SelectedCreditCardId);
            }
        }
    
        public bool HasCreditCards 
		{
            get 
			{
                return CreditCards.Any();
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

        public ListItem<Guid>[] GetCreditCardListItems ()
        {
            return CreditCards.Select(x=> new ListItem<Guid> { Id = x.CreditCardId, Display = x.FriendlyName }).ToArray();
        }

		public ICommand NavigateToCreditCardsList
        {
			get 
			{
			    return this.GetCommand (()=>
				{
					if(CreditCards.Count == 0)
					{
							ShowSubViewModel<CreditCardAddViewModel,CreditCardInfos>(new { showInstructions = false }, newCreditCard => InvokeOnMainThread(()=>
						{
							CreditCards.Add (new CreditCardDetails
								{
								   CreditCardCompany = newCreditCard.CreditCardCompany,
								   CreditCardId = newCreditCard.CreditCardId,
								   FriendlyName = newCreditCard.FriendlyName,
								   Last4Digits = newCreditCard.Last4Digits
								});
							SelectedCreditCardId = newCreditCard.CreditCardId;
							//save as default if none
							if(!_accountService.CurrentAccount.DefaultCreditCard.HasValue)
							{
								var account = _accountService.CurrentAccount;
								account.DefaultCreditCard = newCreditCard.CreditCardId;
								_accountService.UpdateSettings(account.Settings, newCreditCard.CreditCardId, account.DefaultTipPercent);
							}
						}));
					}
					else
					{
						ShowSubViewModel<CreditCardsListViewModel, Guid>(null, result => 
						{
							if(result != default(Guid))
							{
								SelectedCreditCardId = result;

								//Reload credit cards in case the credit card list has changed (add/remove)
								LoadCreditCards();
							}
               			});
					}
				});
			}
		}
    
        public Task LoadCreditCards ()
        {
            var task = Task.Factory.StartNew(() => 
				{
					var cards = _accountService.GetCreditCards();
                    InvokeOnMainThread(delegate {
						CreditCards.Clear();
                        foreach (var card in cards) {
                            CreditCards.Add(card);
                        }
                        // refresh selected credit card
						RaisePropertyChanged(()=>SelectedCreditCard);
                    });
            }).HandleErrors();
        
            return task;
        }
    }
}