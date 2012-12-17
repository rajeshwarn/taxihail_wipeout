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
using System.Collections.Generic;


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
                    FirePropertyChanged("SelectedCreditCard");
                }

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

        public double? TipDouble {
            get {
                double doubleValue;
                return double.TryParse (Tip, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out doubleValue)
                    ? doubleValue
                    : default(double?);
            }
        }



        public apcurium.MK.Common.Entity.ListItem<Guid>[] GetCreditCardListItems ()
        {
            return CreditCards.Select(x=> new ListItem<Guid> { Id = x.CreditCardId, Display = x.FriendlyName }).ToArray();
        }

		public IMvxCommand TogglePercent {
			get {
				return GetCommand (()=>{
					IsTipInPercent = !IsTipInPercent;
				});
			}
		}

        public IMvxCommand NavigateToCreditCardsList {
            get {
                return GetCommand (()=>{
                    RequestSubNavigate<CreditCardsListViewModel, Guid>(null, result => {
                        if(result != default(Guid))
                        {
                            this.SelectedCreditCardId = result;

                            //Reload credit cards in case the selected card has just been added
                            this.LoadCreditCards();
                        }
                    });
                });
            }
        }

        public bool ValidatePaymentSettings ()
        {

            if(!string.IsNullOrWhiteSpace(Tip) && !TipDouble.HasValue)
            {
                base.MessageService.ShowMessage (Resources.GetString ("PaymentDetails.InvalidDataTitle"), Resources.GetString ("PaymentDetails.InvalidTipAmount"));
                return false;
            }
                        
            return true;
        }

        private Task LoadCreditCards ()
        {
            var task = Task.Factory.StartNew(() => {
                
                var cards = _accountService.GetCreditCards();
                InvokeOnMainThread(delegate {
                    CreditCards.Clear();
                    foreach (var card in cards) {
                        CreditCards.Add(card);
                    }
                    // refresh selected credit card
                    FirePropertyChanged("SelectedCreditCard");
                });
            }).HandleErrors();

            return task;
        }
    }
}

