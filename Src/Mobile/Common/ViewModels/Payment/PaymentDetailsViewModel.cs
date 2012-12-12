using System;
using System.Collections.ObjectModel;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using System.Linq;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Interfaces.Commands;
using System.Globalization;
using apcurium.MK.Booking.Mobile.Extensions;


namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PaymentDetailsViewModel: BaseSubViewModel<PaymentDetailsViewModelResult>,
        IMvxServiceConsumer<IAccountService>
    {
        private IAccountService _accountService;
        public PaymentDetailsViewModel (string messageId, PaymentDetailsViewModelResult paymentDetails): base(messageId)
        {
            _accountService = this.GetService<IAccountService>();
            CreditCards.CollectionChanged += (sender, e) => FirePropertyChanged("HasCreditCards");
            LoadCreditCards();

            SelectedCreditCardId = paymentDetails.CreditCardId;
            TipAmount = paymentDetails.TipAmount.HasValue ? paymentDetails.TipAmount.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
            TipPercent = paymentDetails.TipPercent.HasValue ? paymentDetails.TipPercent.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
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
                    FirePropertyChanged("SelectedCreditCardId");
                    FirePropertyChanged("SelectedCreditCardName");
                }

            }
        }

        public string SelectedCreditCardName {
            get{ 
                var selectedCreditCard = this.CreditCards.FirstOrDefault(x=>x.CreditCardId == SelectedCreditCardId);
                if(selectedCreditCard == null)
                {
                    return null;
                }
                return selectedCreditCard.FriendlyName;
            }
        }

        public bool HasCreditCards {
            get {
                return CreditCards.Any();
            }
        }

        private string _tipAmount;
        public string TipAmount {
            get{ return _tipAmount; }
            set {
                if(value != _tipAmount)
                {
                    _tipAmount = value;
                    FirePropertyChanged("TipAmount");
                }
            }
        }

        private string _tipPercent;
        public string TipPercent {
            get{ return _tipPercent; }
            set {
                if(value != _tipPercent)
                {
                    _tipPercent = value;
                    FirePropertyChanged("TipPercent");
                }
            }
        }


        public apcurium.MK.Common.Entity.ListItem<Guid>[] GetCreditCardListItems ()
        {
            return CreditCards.Select(x=> new ListItem<Guid> { Id = x.CreditCardId, Display = x.FriendlyName }).ToArray();
        }

        public IMvxCommand SaveCommand
        {
            get
            {
                
                return GetCommand(() => 
                                  {
                    ReturnResult(new PaymentDetailsViewModelResult
                    {
                        CreditCardId = this.SelectedCreditCardId,
                        TipAmount = GetDecimalValue(this.TipAmount).GetValueOrDefault(),
                        TipPercent = GetDecimalValue(this.TipPercent).GetValueOrDefault()
                    });
                });
            }
        }

        private bool ValidatePaymentSettings ()
        {
            if (string.IsNullOrEmpty (TipAmount) 
                || string.IsNullOrEmpty (TipPercent)) {
                base.MessageService.ShowMessage (Resources.GetString ("PaymentSettings.InvalidDataTitle"), Resources.GetString ("PaymentSettings.EmptyTipAmount"));
                return false;
            }
            if (!string.IsNullOrEmpty (TipAmount)) {

                if(!GetDecimalValue(TipAmount).HasValue)
                {
                    base.MessageService.ShowMessage (Resources.GetString ("PaymentSettings.InvalidDataTitle"), Resources.GetString ("PaymentSettings.InvalidTipAmount"));
                    return false;
                }
            }
            if (!string.IsNullOrEmpty (TipPercent)) {
                
                if(!GetDecimalValue(TipPercent).HasValue)
                {
                    base.MessageService.ShowMessage (Resources.GetString ("PaymentSettings.InvalidDataTitle"), Resources.GetString ("PaymentSettings.InvalidTipPercent"));
                    return false;
                }
            }
                        
            return true;
        }


        private Task LoadCreditCards ()
        {
            return Task.Factory.StartNew(() => {
                
                var cards = _accountService.GetCreditCards();
                CreditCards.Clear();
                foreach (var card in cards) {
                    CreditCards.Add(card);
                }
            }).HandleErrors();
        }

        private decimal? GetDecimalValue(string value)
        {
            decimal decimalValue;
            return decimal.TryParse (value, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimalValue)
                        ? decimalValue
                        : default(decimal?);
        }


    }
}

