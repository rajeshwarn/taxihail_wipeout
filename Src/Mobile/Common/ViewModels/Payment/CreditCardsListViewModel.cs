using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
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
            _removeCreditCardToken = MessengerHub.Subscribe<RemoveCreditCard>(creditCardId => RemoveCreditCard(creditCardId.Content));
        }

        public override void Stop ()
        {
            base.Stop ();

            if (_removeCreditCardToken != null) {
                MessengerHub.Unsubscribe<RemoveCreditCard>(_removeCreditCardToken);
            }
        }

        public void RemoveCreditCard (Guid creditCardId)
        {
            MessageService.ShowMessage(Resources.GetString("RemoveCreditCardTitle"),
                Resources.GetString("RemoveCreditCardMessage"),
                Resources.GetString("YesButton"),
                () =>
                {
                    AccountService.RemoveCreditCard(creditCardId);
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
                Resources.GetString("CancelBoutton"),
                () => { });
                                                                  
        }

        public Task LoadCreditCards()
        {
            return Task.Factory.StartNew(() =>
            {
                var creditCards = AccountService.GetCreditCards().ToList();
                creditCards.Add(new CreditCardDetails
                {
                    FriendlyName = Resources.GetString("AddCreditCardTitle"),
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

        public IMvxCommand NavigateToAddOrSelect
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

        public IMvxCommand NavigateToAddCreditCard
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

        public IMvxCommand SelectCreditCardAndBackToSettings
        {
            get
            {
                return GetCommand<Guid>(ReturnResult);
            }
        }
    }
}