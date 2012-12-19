using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.Data;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CreditCardsListViewModel : BaseSubViewModel<Guid>,
        IMvxServiceConsumer<IAccountService>
    {

        private ObservableCollection<CreditCardViewModel> _creditCards;

        public ObservableCollection<CreditCardViewModel> CreditCards
        {
            get { return _creditCards; }
            set { this._creditCards = value; FirePropertyChanged("CreditCards"); }
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
            var accountService = this.GetService<IAccountService>();
            LoadCreditCards();
            this.MessengerHub.Subscribe<CreditCardRemoved>(creditCardId => RemoveCreditCard(creditCardId.Content));
        }

        public void RemoveCreditCard(Guid creditCardId)
        {
            this.MessageService.ShowMessage(this.Resources.GetString("RemoveCreditCardTitle"),
                                                                       this.Resources.GetString("RemoveCreditCardMessage"),
                                                                       this.Resources.GetString("YesButton"),
                                                                       () =>
                                                                       {
                                                                           TinyIoCContainer.Current.Resolve<IAccountService>()
                                                                                .RemoveCreditCard(creditCardId);
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
                                                                           this.Resources.GetString("CancelBoutton"),
                                                                           () => { });
                                                                                                                                                                                                                                        
                                                                  
        }

        public Task LoadCreditCards()
        {
            return Task.Factory.StartNew(() =>
            {
                var creditCards = TinyIoCContainer.Current.Resolve<IAccountService>().GetCreditCards().ToList();
                creditCards.Add(new CreditCardDetails
                {
                    FriendlyName = Resources.GetString("AddCreditCardTitle"),
                });
                CreditCards = new ObservableCollection<CreditCardViewModel>(creditCards.Select(x => new CreditCardViewModel()
                {
                    CreditCardDetails = x,
                    IsAddNew = x.CreditCardId.IsNullOrEmpty(),
                    ShowPlusSign = x.CreditCardId.IsNullOrEmpty(),
                    IsFirst = x.Equals(creditCards.First()),
                    IsLast = x.Equals(creditCards.Last()),
                    Picture = x.CreditCardCompany
                }));
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
                                                                   this.NavigateToAddCreditCard.Execute();
                                                               }
                                                               else
                                                               {
                                                                   this.SelectCreditCardAndBackToSettings.Execute(creditCard.CreditCardDetails.CreditCardId);
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

                                                                                                                 //var lastElem = CreditCards.ElementAt(CreditCards.Count - 1);

                                                                                                                // CreditCards.Remove(lastElem);
				                                                                                                 CreditCards.Insert(CreditCards.Count-1,new CreditCardViewModel()
                                                                                                                 {
                                                                                                                     CreditCardDetails = new CreditCardDetails()
                                                                                                                                             {
                                                                                                                                                 CreditCardCompany = newCreditCard.CreditCardCompany,
                                                                                                                                                 FriendlyName = newCreditCard.FriendlyName,
                                                                                                                                                 Last4Digits = newCreditCard.Last4Digits
                                                                                                                                             },
                                                                                                                     Picture = newCreditCard.CreditCardCompany
                                                                                                                 });
                                                                                                               //  CreditCards.Add(lastElem);
                                                                                                                 FirePropertyChanged("CreditCards");
                                                                                                                 });                                                                                         
                                                                                                            CreditCards[0].IsFirst=true;
                                                                                                            CreditCards.LastOrDefault().IsFirst=false;                        
                                                                                                            CreditCards.LastOrDefault().IsLast = true;
                                                                                                            CreditCards.LastOrDefault().IsAddNew = true;
                                                                                                            CreditCards = new ObservableCollection<CreditCardViewModel>(CreditCards);
				                                                                                             }));
                
            }
        }

        public IMvxCommand SelectCreditCardAndBackToSettings
        {
            get
            {
                return GetCommand<Guid>(creditCardId => this.ReturnResult(creditCardId));
            }
        }
    }
}