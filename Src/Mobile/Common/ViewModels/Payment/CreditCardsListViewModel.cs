using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Extensions;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class CreditCardsListViewModel : BaseSubViewModel<Guid>
    {
        private TinyMessageSubscriptionToken _removeCreditCardToken;
        private ObservableCollection<CreditCardViewModel> _creditCards;

        public ObservableCollection<CreditCardViewModel> CreditCards
        {
            get { return _creditCards; }
            set { _creditCards = value; FirePropertyChanged("CreditCards"); }
        }

        private bool _hasCards;
        public bool HasCards
        {
            get
            {
                return _hasCards;
            }
            set
            {
                if (value != _hasCards)
                {
                    _hasCards = value;
                    FirePropertyChanged("HasCards");
                }
            }
        }



        public CreditCardsListViewModel(string messageId):base(messageId)
        {
            LoadCreditCards();
        }

        public override void Start (bool firstStart = false)
        {
            base.Start (firstStart);
            _removeCreditCardToken = this.Services().MessengerHub.Subscribe<RemoveCreditCard>(creditCardId => RemoveCreditCard(creditCardId.Content));
        }

        public override void Stop ()
        {
            base.Stop ();

            if (_removeCreditCardToken != null) {
                this.Services().MessengerHub.Unsubscribe<RemoveCreditCard>(_removeCreditCardToken);
            }
        }

        public void RemoveCreditCard (Guid creditCardId)
        {
            this.Services().Message.ShowMessage(this.Services().Localize["RemoveCreditCardTitle"],
                this.Services().Localize["RemoveCreditCardMessage"],
                this.Services().Localize["YesButton"],
                () =>
                {
                    this.Services().Account.RemoveCreditCard(creditCardId);
                    var creditCardToRemove = CreditCards.FirstOrDefault(c => c.CreditCardDetails.CreditCardId.Equals(creditCardId));
                    if (creditCardToRemove != null)
                    {
                        InvokeOnMainThread(() => CreditCards.Remove(creditCardToRemove));
                        FirePropertyChanged("CreditCards");
                    }
                    CreditCards[0].IsFirst = true;

                    if (CreditCards.Count.Equals(1))
                    {
                        CreditCards[0].IsLast = true;
                        CreditCards[0].IsAddNew = true;
                    }
                    CreditCards = new ObservableCollection<CreditCardViewModel>(CreditCards);
                },
                this.Services().Localize["CancelButton"],
                () => { });
                                                                  
        }

        public Task LoadCreditCards()
        {
            return Task.Factory.StartNew(() =>
            {
                var creditCards = this.Services().Account.GetCreditCards().ToList();
                creditCards.Add(new CreditCardDetails
                {
                    FriendlyName = this.Services().Localize["AddCreditCardTitle"],
                });
                CreditCards = new ObservableCollection<CreditCardViewModel>(creditCards.Select(x => new CreditCardViewModel
                {
                    CreditCardDetails = x,
                    IsAddNew = x.CreditCardId.IsNullOrEmpty(),
                    ShowPlusSign = x.CreditCardId.IsNullOrEmpty(),
                    IsFirst = x.Equals(creditCards.First()),
                    IsLast = x.Equals(creditCards.Last()),
                    Picture = x.CreditCardCompany,
                }));
                //todo utiliser une propriété calculée
                HasCards = CreditCards.Any();
            });
        }

        public AsyncCommand<CreditCardViewModel> NavigateToAddOrSelect
        {
            get
            {
                return GetCommand<CreditCardViewModel>(creditCard =>
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

        public AsyncCommand NavigateToAddCreditCard
        {
            get
            {
				return GetCommand(() => RequestSubNavigate<CreditCardAddViewModel,CreditCardInfos>(null, newCreditCard =>
                {
                    InvokeOnMainThread(()=>
                    {
                         CreditCards.Insert(CreditCards.Count-1,new CreditCardViewModel
                         {
                             CreditCardDetails = new CreditCardDetails
                             {
                                 CreditCardCompany = newCreditCard.CreditCardCompany,
                                 CreditCardId = newCreditCard.CreditCardId,
                                 FriendlyName = newCreditCard.FriendlyName,
                                 Last4Digits = newCreditCard.Last4Digits
                             },
                             Picture = newCreditCard.CreditCardCompany
                        });
                        FirePropertyChanged("CreditCards");
                    });                                                                                         
                    CreditCards[0].IsFirst=true;
                    CreditCards.Last().IsFirst=false;                        
                    CreditCards.Last().IsLast = true;
                    CreditCards.Last().IsAddNew = true;
                    CreditCards = new ObservableCollection<CreditCardViewModel>(CreditCards);
                 }));
                
            }
        }

        public AsyncCommand<Guid> SelectCreditCardAndBackToSettings
        {
            get
            {
                return GetCommand<Guid>(ReturnResult);
            }
        }
    }
}