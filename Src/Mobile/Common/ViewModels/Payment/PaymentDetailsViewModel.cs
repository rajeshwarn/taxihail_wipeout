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
    public class PaymentDetailsViewModel: BaseSubViewModel<PaymentInformation>,
        IMvxServiceConsumer<IAccountService>
    {
        private IAccountService _accountService;
        public PaymentDetailsViewModel (string messageId, PaymentInformation paymentDetails): base(messageId)
        {
            _accountService = this.GetService<IAccountService>();
            CreditCards.CollectionChanged += (sender, e) => FirePropertyChanged("HasCreditCards");
            LoadCreditCards();

            SelectedCreditCardId = paymentDetails.CreditCardId.GetValueOrDefault();
            IsTipInPercent = paymentDetails.TipPercent.HasValue;
            Tip = paymentDetails.TipPercent.HasValue 
                ? paymentDetails.TipPercent.Value.ToString(CultureInfo.InvariantCulture) 
                : paymentDetails.TipAmount.HasValue 
                    ? paymentDetails.TipAmount.Value.ToString(CultureInfo.InvariantCulture) 
                    : string.Empty;
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

        private bool _isTipInPercent = true;
        public bool IsTipInPercent {
            get {
                return _isTipInPercent;
            }
            set {
                if(value != _isTipInPercent)
                {
                    _isTipInPercent = value;
                    FirePropertyChanged("IsTipInPercent");
                }
            }
        }

        private string _tip;
        public string Tip {
            get {
                return _tip;
            }
            set {
                if(value != _tip)
                {
                    _tip = value;
                    FirePropertyChanged("Tip");
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
                    var tipValue = GetDoubleValue(this.Tip).GetValueOrDefault();
                    
                    ReturnResult(new PaymentInformation
                    {
                        CreditCardId = this.SelectedCreditCardId,
                        TipAmount = IsTipInPercent ? default(double?) : tipValue,
                        TipPercent = IsTipInPercent ? tipValue : default(double?),
                    });
                });
            }
        }

        private bool ValidatePaymentSettings ()
        {
            if (string.IsNullOrEmpty (Tip) ) {
                base.MessageService.ShowMessage (Resources.GetString ("PaymentSettings.InvalidDataTitle"), Resources.GetString ("PaymentSettings.EmptyTipAmount"));
                return false;
            }

            if(!GetDoubleValue(Tip).HasValue)
            {
                base.MessageService.ShowMessage (Resources.GetString ("PaymentSettings.InvalidDataTitle"), Resources.GetString ("PaymentSettings.InvalidTipAmount"));
                return false;
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

        private double? GetDoubleValue(string value)
        {
            double doubleValue;
            return double.TryParse (value, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out doubleValue)
                        ? doubleValue
                        : default(double?);
        }


    }
}

