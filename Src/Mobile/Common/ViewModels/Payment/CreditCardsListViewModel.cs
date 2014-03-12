using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class CreditCardsListViewModel : PageViewModel, ISubViewModel<Guid>
    {
		private readonly IAccountService _accountService;

		public CreditCardsListViewModel(IAccountService accountService)
		{
			_accountService = accountService;
		}

		private TinyMessageSubscriptionToken _removeCreditCardToken;
        private ObservableCollection<CreditCardViewModel> _creditCards;

        public ObservableCollection<CreditCardViewModel> CreditCards
        {
            get { return _creditCards; }
            set
			{ 
				_creditCards = value;
				RaisePropertyChanged(); 
			}
        }

		public override void Start()
        {
            LoadCreditCards();
        }

        public override void OnViewStarted (bool firstStart = false)
        {
            base.OnViewStarted (firstStart);
            _removeCreditCardToken = this.Services().MessengerHub.Subscribe<RemoveCreditCard>(creditCardId => RemoveCreditCard(creditCardId.Content));
        }

        public override void OnViewStopped ()
        {
            base.OnViewStopped ();

            if (_removeCreditCardToken != null) {
                this.Services().MessengerHub.Unsubscribe<RemoveCreditCard>(_removeCreditCardToken);
            }
        }

        public void RemoveCreditCard (Guid creditCardId)
        {
			if (this.Settings.CreditCardIsMandatory && CreditCards.Count(x => !x.IsAddNew) == 1)
			{
				this.Services().Message.ShowMessage(this.Services().Localize["CreditCardRemoveErrorTitle"], this.Services().Localize["CreditCardRemoveErrorMessage"]);
				return;
			}

            this.Services().Message.ShowMessage(this.Services().Localize["RemoveCreditCardTitle"],
									            this.Services().Localize["RemoveCreditCardMessage"],
									            this.Services().Localize["YesButton"],
			            () =>
			            {
							_accountService.RemoveCreditCard(creditCardId);
				            var creditCardToRemove = CreditCards.FirstOrDefault(c => c.CreditCardDetails.CreditCardId.Equals(creditCardId));
				            if (creditCardToRemove != null)
				            {
				                InvokeOnMainThread(() => CreditCards.Remove(creditCardToRemove));
								RaisePropertyChanged(() => CreditCards);
				            }

				            CreditCards = new ObservableCollection<CreditCardViewModel>(CreditCards);
							if(_accountService.CurrentAccount.DefaultCreditCard == creditCardId)
							{
								var account = _accountService.CurrentAccount;
								account.DefaultCreditCard = null;
								_accountService.UpdateSettings(account.Settings, null, account.DefaultTipPercent);								
							}

			            },
			            this.Services().Localize["Cancel"],
			            () => { });
                                                                  
        }

        public Task LoadCreditCards()
        {
            return Task.Factory.StartNew(() =>
            {
				var creditCards = _accountService.GetCreditCards().ToList();
				creditCards.Insert(0, new CreditCardDetails
                {
                    FriendlyName = this.Services().Localize["AddCreditCardTitle"],
                });
                CreditCards = new ObservableCollection<CreditCardViewModel>(creditCards.Select(x => new CreditCardViewModel
                {
                    CreditCardDetails = x,
					Picture = x.CreditCardCompany != null 
							  	? x.CreditCardCompany.ToLower().Replace(" ", "_") 
                              : string.Empty,
					IsLast = creditCards.Last().CreditCardId == x.CreditCardId
                }));
            });
        }

		public ICommand NavigateToAddOrSelect
        {
            get
            {
                return this.GetCommand<CreditCardViewModel>(creditCard =>
                {
                    if (creditCard.IsAddNew)
                    {
                        NavigateToAddCreditCard.Execute();
                    }
                    else
                    {
                        SelectCreditCardAndBackToSettings.Execute(creditCard.CreditCardDetails.CreditCardId);
                    }
                });
            }
        }

		public ICommand NavigateToAddCreditCard
        {
            get
            {
				return this.GetCommand(() => ShowSubViewModel<CreditCardAddViewModel,CreditCardInfos>(null, newCreditCard =>
                {
                    InvokeOnMainThread(()=>
                    {
						 CreditCards.Insert(1, new CreditCardViewModel
                         {
                             CreditCardDetails = new CreditCardDetails
                             {
                                 CreditCardCompany = newCreditCard.CreditCardCompany,
                                 CreditCardId = newCreditCard.CreditCardId,
                                 FriendlyName = newCreditCard.FriendlyName,
                                 Last4Digits = newCreditCard.Last4Digits
                             },
							 Picture = newCreditCard.CreditCardCompany != null 
							 	? newCreditCard.CreditCardCompany.ToLower().Replace(" ", "_")
								: string.Empty
                        });
						RaisePropertyChanged(() => CreditCards);
                    });                                                                                         		
                    CreditCards = new ObservableCollection<CreditCardViewModel>(CreditCards);
                 }));
            }
        }

		public ICommand SelectCreditCardAndBackToSettings
        {
            get
            {
				return this.GetCommand<Guid>(guid => this.ReturnResult(guid));
            }
        }
    }
}