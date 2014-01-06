using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using System.Collections.ObjectModel;
using apcurium.MK.Booking.Api.Contract.Resources;
using System;
using System.Globalization;
using apcurium.MK.Common.Configuration;
using System.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Data;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;

public class PaymentDetailsViewModel: BaseSubViewModel<PaymentInformation>
{
    public PaymentDetailsViewModel (string messageId, PaymentInformation paymentDetails): base(messageId)
    {

		CreditCards.CollectionChanged += (sender, e) =>
		{
			FirePropertyChanged(() => HasCreditCards);
		};
        LoadCreditCards();
        
        SelectedCreditCardId = paymentDetails.CreditCardId.GetValueOrDefault();
        
        Tip = paymentDetails.TipPercent.HasValue 
            ? paymentDetails.TipPercent.Value 
                : 0;
    }
    
    private readonly ObservableCollection<CreditCardDetails> _creditCards = new ObservableCollection<CreditCardDetails>();
    public ObservableCollection<CreditCardDetails> CreditCards {
        get {
            return _creditCards;
        }
    }
    
    private Guid _selectedCreditCardId;
    public Guid SelectedCreditCardId {
        get{ return _selectedCreditCardId; }
        set{
            if(value != _selectedCreditCardId)
            {
                _selectedCreditCardId = value;
                FirePropertyChanged(()=>SelectedCreditCardId);
                FirePropertyChanged(()=>SelectedCreditCard);
            }
            
        }
    }
    
    public string CurrencySymbol {
        get {
            var culture = new CultureInfo(ConfigurationManager.GetSetting("PriceFormat"));
            return culture.NumberFormat.CurrencySymbol;
        }
    }
    
    public CreditCardDetails SelectedCreditCard {
        get{ 
            return this.CreditCards.FirstOrDefault(x=>x.CreditCardId == SelectedCreditCardId);
        }
    }
    
    public bool HasCreditCards {
        get {
            return CreditCards.Any();
        }
    }
    
    public int Tip 
    { 
        get
        {
            return _tip;
        }
        set{
            _tip = value;
            FirePropertyChanged(()=>Tip);
        }
    }
    
    public int _tip { get; set; }
    
    public apcurium.MK.Common.Entity.ListItem<Guid>[] GetCreditCardListItems ()
    {
        return CreditCards.Select(x=> new ListItem<Guid> { Id = x.CreditCardId, Display = x.FriendlyName }).ToArray();
    }
    
    public IMvxCommand NavigateToCreditCardsList {
        get {
            return GetCommand (()=>{
                
                if(CreditCards.Count == 0)
                {
                    RequestSubNavigate<CreditCardAddViewModel,CreditCardInfos>(null, newCreditCard =>
                                                                               {
                        InvokeOnMainThread(()=>
                                           {
                            CreditCards.Add (new CreditCardDetails
                                             {
                                CreditCardCompany = newCreditCard.CreditCardCompany,
                                CreditCardId = newCreditCard.CreditCardId,
                                FriendlyName = newCreditCard.FriendlyName,
                                Last4Digits = newCreditCard.Last4Digits
                            });
                            this.SelectedCreditCardId = newCreditCard.CreditCardId;
                        });                                                                                         
                        
                    });
                    
                }else{
                    
                    
                    RequestSubNavigate<CreditCardsListViewModel, Guid>(null, result => {
                        if(result != default(Guid))
                        {
                            this.SelectedCreditCardId = result;
                            
                            //Reload credit cards in case the credit card list has changed (add/remove)
                            this.LoadCreditCards();
                        }
                    });
                }
            });
        }
    }
    
    public Task LoadCreditCards ()
    {
        var task = Task.Factory.StartNew(() => {
            
            var cards = AccountService.GetCreditCards();
            InvokeOnMainThread(delegate {
                CreditCards.Clear();
                foreach (var card in cards) {
                    CreditCards.Add(card);
                }
                // refresh selected credit card
                FirePropertyChanged(()=>SelectedCreditCard);
            });
        }).HandleErrors();
        
        return task;
    }
}