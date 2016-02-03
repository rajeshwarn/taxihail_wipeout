using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;


namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class CreditCardAddViewModel : CreditCardBaseViewModel
	{
		private readonly IPaymentService _paymentService;
		private readonly IAccountService _accountService;
	    private readonly IPaymentProviderClientService _paymentProviderClientService;
		private readonly IDeviceCollectorService _deviceCollectorService;

		private bool _hasPaymentToSettle;
		private CreditCardLabelConstants _originalLabel;

		public CreditCardAddViewModel(
			ILocationService locationService,
			IPaymentService paymentService, 
			IAccountService accountService,
			IDeviceCollectorService deviceCollectorService,
			IPaymentProviderClientService paymentProviderClientService)
			: base(locationService, paymentService, accountService)
		{
			_paymentService = paymentService;
			_accountService = accountService;
		    _paymentProviderClientService = paymentProviderClientService;
			_deviceCollectorService = deviceCollectorService;
		}

		private bool _isFromPromotionsView;
		private bool _isFromCreditCardListView;
		private bool _isAddingNew;
		private Guid _creditCardId;
		private int _numberOfCreditCards;
		private string _kountSessionId;

        private int _selectedLabel;
        private bool _isShowPaypalView;

        #region Const and ReadOnly
        private const string Visa = "Visa";
		private const string MasterCard = "MasterCard";
		private const string Amex = "Amex";
		private const string CreditCardGeneric = "Credit Card Generic";
	    private const string Paypal = "Paypal";
		private const string VisaElectron = "Visa Electron";
		private readonly string[] _visaElectronFirstNumbers = { "4026", "417500", "4405", "4508", "4844", "4913", "4917" };
		private const string VisaPattern = "^4[0-9]{12}(?:[0-9]{3})?$";
		private const string MasterPattern = "^5[1-5][0-9]{14}$";
		private const string AmexPattern = "^3[47][0-9]{13}$";
		private const int TipMaxPercent = 100;
		#endregion

		public class DummyVisa
		{
			public static string BraintreeNumber = "4009 3488 8888 1881".Replace(" ", "");
			public static string CmtNumber = "4012 0000 3333 0026".Replace(" ", "");
			public static int AvcCvvCvv2 = 135;
			public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
			public static string ZipCode = "95001";
		}

		public void Init(bool showInstructions = false, 
			bool isMandatory = false, 
			bool hasPaymentToSettle = false, 
			bool isFromPromotionsView = false, 
			bool isFromCreditCardListView = false, 
			bool isAddingNew = false, 
			Guid creditCardId = default(Guid))
		{
			ShowInstructions = showInstructions;
			IsMandatory = isMandatory;

			_isFromPromotionsView = isFromPromotionsView;
			_isFromCreditCardListView = isFromCreditCardListView;
			_isAddingNew = isAddingNew;
			_creditCardId = creditCardId;

		    _hasPaymentToSettle = hasPaymentToSettle;
			_kountSessionId = _deviceCollectorService.GetSessionId();
		}

        CreditCardDetails _currentlyDisplayedCreditCard = null;

        public override async void BaseStart()
        {
            
            using (this.Services().Message.ShowProgress())
            {
                CreditCardCompanies = new List<ListItem>
				{
					new ListItem {Display = Visa, Id = 0},
					new ListItem {Display = MasterCard, Id = 1},
					new ListItem {Display = Amex, Id = 2},
					new ListItem {Display = VisaElectron, Id = 3},
                    new ListItem {Display = Paypal, Id = 4},
                    new ListItem {Display = CreditCardGeneric, Id = 5},
				};

                ExpirationYears = new List<ListItem>();
                for (var i = 0; i <= 15; i++)
                {
                    ExpirationYears.Add(new ListItem { Id = DateTime.Today.AddYears(i).Year, Display = DateTime.Today.AddYears(i).Year.ToString(CultureInfo.InvariantCulture) });
                }

                ExpirationMonths = new List<ListItem>
                    {
                        new ListItem {Display = this.Services().Localize["January"], Id = 1},
                        new ListItem {Display = this.Services().Localize["February"], Id = 2},
                        new ListItem {Display = this.Services().Localize["March"], Id = 3},
                        new ListItem {Display = this.Services().Localize["April"], Id = 4},
                        new ListItem {Display = this.Services().Localize["May"], Id = 5},
                        new ListItem {Display = this.Services().Localize["June"], Id = 6},
                        new ListItem {Display = this.Services().Localize["July"], Id = 7},
                        new ListItem {Display = this.Services().Localize["August"], Id = 8},
                        new ListItem {Display = this.Services().Localize["September"], Id = 9},
                        new ListItem {Display = this.Services().Localize["October"], Id = 10},
                        new ListItem {Display = this.Services().Localize["November"], Id = 11},
                        new ListItem {Display = this.Services().Localize["December"], Id = 12}
                    };

                Data = new CreditCardInfos();

                try
                {
                    var creditCards = (await _accountService.GetCreditCards()).ToList();
                    _numberOfCreditCards = creditCards.Count;

                    _currentlyDisplayedCreditCard = _creditCardId == default(Guid)
                        ? await _accountService.GetDefaultCreditCard()
                        : creditCards.First(c => c.CreditCardId == _creditCardId);
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(ex.Message, ex.ToString());
                    this.Services().Message.ShowMessage(this.Services().Localize["Error"], this.Services().Localize["PaymentLoadError"]);
                }

                if (_currentlyDisplayedCreditCard == null || _isAddingNew)
                {
                    AddNewCreditCard();
                }
                else
                {
                    IsEditing = true;

                    EditCurrentPaymentMethod(_currentlyDisplayedCreditCard);
                }

                IsShowPaypalView = Data.CreditCardCompany == Paypal;
                
                RaisePropertyChanged(() => Data);
                RaisePropertyChanged(() => CreditCardNumber);
                RaisePropertyChanged(() => CanDeleteCreditCard);
                RaisePropertyChanged(() => IsPaypalEnabled);
                RaisePropertyChanged(() => CanSetCreditCardAsDefault);

                if (_hasPaymentToSettle)
                {
                    return;
                }

                if (!_isFromCreditCardListView)
                {
                    await GoToOverduePayment();
                }
            }
        }

	    private void EditCurrentPaymentMethod(CreditCardDetails creditCard)
	    {
	        Data.CreditCardId = creditCard.CreditCardId;

	        Data.CardNumber = "************" + creditCard.Last4Digits;

	        Data.NameOnCard = creditCard.NameOnCard;
	        Data.CreditCardCompany = creditCard.CreditCardCompany;
	        Data.Label = creditCard.Label;
	        Data.ZipCode = creditCard.ZipCode;

	        ExpirationMonth = creditCard.ExpirationMonth.HasValue() ? int.Parse(creditCard.ExpirationMonth) : (int?) null;
	        ExpirationYear = creditCard.ExpirationYear.HasValue() ? int.Parse(creditCard.ExpirationYear) : (int?) null;

	        var id = CreditCardCompanies
	            .Where(x => x.Display == creditCard.CreditCardCompany)
	            .Select(x => x.Id)
	            .FirstOrDefault();

	        if (id != null)
	        {
	            CreditCardType = (int) id;
	        }

	        _originalLabel = creditCard.Label;
	    }

	    private void AddNewCreditCard()
	    {
	        Data.NameOnCard = _accountService.CurrentAccount.Name;
	        Data.Label = CreditCardLabelConstants.Personal;

	        var id = CreditCardCompanies.Find(x => x.Display == CreditCardGeneric).Id;
	        CreditCardType = id ?? -1;

#if DEBUG
	        if (PaymentSettings.PaymentMode == PaymentMethod.Braintree)
	        {
	            CreditCardNumber = DummyVisa.BraintreeNumber;
	        }
	        else
	        {
	            CreditCardNumber = DummyVisa.CmtNumber;
	        }

	        Data.CCV = DummyVisa.AvcCvvCvv2 + "";
	        Data.ZipCode = DummyVisa.ZipCode;

	        ExpirationMonth = DummyVisa.ExpirationDate.Month;
	        ExpirationYear = DummyVisa.ExpirationDate.Year;
#endif
	    }

	    public bool IsPaypalEnabled
	    {
	        get { return PaymentSettings.PaymentMode == PaymentMethod.Braintree
                    && PaymentSettings.BraintreeClientSettings.EnablePayPal; }
	    }

		public string CreditCardNumber
		{
			get{ return Data.CardNumber; }
			set
			{
				Data.CardNumber = value;
				DetermineCompany (value);

				RaisePropertyChanged();
			}
		}

		private int _creditCardType;
		public int CreditCardType 
		{
			get { return _creditCardType; }
			set 
			{
				_creditCardType = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => CreditCardTypeName);
				RaisePropertyChanged(() => CreditCardImagePath);
			}
		}

	    public bool IsShowingCancel
	    {
	        get { return _isShowingCancel; }
	        set
	        {
	            _isShowingCancel = value;
	            RaisePropertyChanged();
	        }
	    }

	    public string CreditCardTypeName 
		{ 
			get 
			{
				var type = CreditCardCompanies.FirstOrDefault(x=>x.Id == CreditCardType);
				return type == null ? null : type.Display;
			}
		}

		public string CreditCardImagePath 
		{ 
			get 
			{
				var type = CreditCardCompanies.FirstOrDefault(x=>x.Id == CreditCardType);
				return type == null ? null : type.Image;
			}
		}

		public int? ExpirationYear 
		{
			get 
			{
				return string.IsNullOrEmpty(Data.ExpirationYear) 
					? (int?)null
						: int.Parse(Data.ExpirationYear);
			}
			set 
			{
				Data.ExpirationYear = value.SelectOrDefault(instance => instance.ToString(), string.Empty);
				RaisePropertyChanged();
				RaisePropertyChanged(() => ExpirationYearDisplay);
			}
		}

		public int? ExpirationMonth 
		{
			get 
			{
				return string.IsNullOrEmpty(Data.ExpirationMonth) 
					? (int?)null
						: int.Parse(Data.ExpirationMonth);
			}
			set 
			{
				Data.ExpirationMonth = value.SelectOrDefault(instance => instance.ToString(), string.Empty);

                RaisePropertyChanged();
				RaisePropertyChanged(() => ExpirationMonthDisplay);
			}
		}

		public string ExpirationMonthDisplay 
		{
			get 
			{
				var month = ExpirationMonths.FirstOrDefault(x => x.Id == ExpirationMonth);
				return month == null ? "" : month.Display;
			}
		}

		public string ExpirationYearDisplay 
		{
			get 
			{
				var year = ExpirationYears.FirstOrDefault(x => x.Id == ExpirationYear);
				return year == null ? "" : year.Display;
			}
		}

		public List<ListItem> CreditCardCompanies { get; set; }
		public List<ListItem> ExpirationYears { get; set; }
		public List<ListItem> ExpirationMonths { get; set; }
		public bool ShowInstructions { get; set; }
		public bool IsMandatory { get; set; }

		private CreditCardInfos _data;
		public CreditCardInfos Data 
		{ 
			get { return _data; }
			set
			{
				_data = value;
				RaisePropertyChanged ();
				RaisePropertyChanged (() => CreditCardNumber);

			}
		}

		private bool _isEditing;
	    private bool _isShowingCancel;

	    public bool IsEditing
		{
			get { return _isEditing; }
			set
			{
				if (_isEditing != value)
				{
					_isEditing = value;
					RaisePropertyChanged();
					RaisePropertyChanged(() => CreditCardSaveButtonDisplay);
				}
			}
		}

		public bool CanDeleteCreditCard
		{
			get { return IsEditing && !IsShowingCancel &&(!PaymentSettings.CreditCardIsMandatory  || PaymentSettings.CreditCardIsMandatory && _numberOfCreditCards > 1); }
		}

		public string CreditCardSaveButtonDisplay
		{
			get
			{
				return IsEditing ? this.Services().Localize["Modify"] : this.Services().Localize["Save"];
			}
		}

		public bool CanChooseTip
		{
			get
			{
                return (PaymentSettings.IsPayInTaxiEnabled || PaymentSettings.PayPalClientSettings.IsEnabled) && !_isFromCreditCardListView && !IsMandatory;
			}
		}

		public bool CanChooseLabel
		{
			get
			{
				return _isFromCreditCardListView;
			}
		}

		public bool CanScanCreditCard
		{
			get
			{
				return _isAddingNew || IsMandatory;
			}
		}

		public bool CanSetCreditCardAsDefault
		{
			get
			{
				return !_isAddingNew && _isFromCreditCardListView && Data.CreditCardId != _accountService.CurrentAccount.DefaultCreditCard.CreditCardId;
			}
		}

	    public bool IsShowPaypalView
	    {
	        get { return _isShowPaypalView; }
	        set
	        {
	            _isShowPaypalView = value;
	            RaisePropertyChanged();
                RaisePropertyChanged(() => IsShowUseCreditCardButton);
	        }
	    }

	    public bool IsShowUseCreditCardButton
	    {
	        get { return IsShowPaypalView && Settings.MaxNumberOfCardsOnFile == 1; }
	    }

	    public int SelectedLabel
		{
			get
			{
				return _selectedLabel;
			}
			set
			{
				_selectedLabel = value;
			}
		}

		public ICommand SetAsDefault 
		{ 
			get
			{
				return this.GetCommand(async() =>
					{
						using (this.Services ().Message.ShowProgress ())
						{
							var updated = await _accountService.UpdateDefaultCreditCard(Data.CreditCardId);
							if(updated)
							{
								Close(this);
							}
							else
							{
								this.Services().Message.ShowMessage(null, this.Services().Localize["SetCreditCardAsDefault_Error"]);
							}
						}
					});
			}
		}
        

		public ICommand SaveCreditCardCommand 
		{ 
			get
			{
				return this.GetCommand(() => SaveCreditCard() );
			} 
		}

	    public ICommand CancelCommand
	    {
	        get
	        {
	            return this.GetCommand(() =>
	            {
                    IsShowPaypalView = true;

	                IsShowingCancel = false;
                    
                    EditCurrentPaymentMethod(_currentlyDisplayedCreditCard);

                    RaisePropertyChanged(() => Data);
                });
	        }
	    }

		public ICommand DeleteCreditCardCommand
		{
			get
			{
				return this.GetCommand(async () =>
					{
						var tcs = new TaskCompletionSource<bool>();

						var localize = this.Services().Localize;

						this.Services().Message.ShowMessage(
							localize["DeleteCreditCardTitle"],
							localize["DeleteCreditCard"],
							localize["Delete"], () => tcs.SetResult(true),
							localize["Cancel"], () => tcs.SetResult(false));

						if (await tcs.Task)
						{
							try
							{
								using (this.Services().Message.ShowProgress())
								{
									await DeleteCreditCard();
								}
							}
							catch (Exception ex)
							{
								Logger.LogError(ex);
								this.Services().Message.ShowMessage(localize["CreditCardRemoveErrorTitle"], localize["CreditCardRemoveErrorScheduledOrderMessage"]);                        
							}
						}
					});
			}
		}

	    public bool HidePaypalButton
	    {
	        get { return PaymentSettings.PaymentMode != PaymentMethod.Braintree; }
	    }

	    public ICommand ShowCreditCardViewCommand
	    {
	        get
	        {
	            return this.GetCommand(() =>
	            {
	                IsShowPaypalView = false;

                    IsShowingCancel = true;

                    AddNewCreditCard();

                    RaisePropertyChanged(() => Data);
                    RaisePropertyChanged(() => CanDeleteCreditCard);
                });
	        }
	    }

	    public ICommand UsePaypalCommand
	    {
	        get
	        {
	            return this.GetCommand(async () =>
	            {
					var messageService = this.Services().Message;

					try
					{
						using(messageService.ShowProgress())
						{
							var clientToken = await _paymentService.GenerateClientTokenResponse();

							var paymentNonce = await _paymentProviderClientService.GetPayPalNonce(clientToken.ClientToken);

                            var creditCardId = Data.CreditCardId == Guid.Empty
                                ? (Guid?)null
                                : Data.CreditCardId;

                            await _paymentService.AddPaymentMethod(paymentNonce, PaymentMethods.Paypal, creditCardId);
						}
						CloseView();
					}
					catch (Exception ex)
					{
						var localize = this.Services().Localize;
						messageService.ShowMessage(localize["PaypalError_Title"], localize["PaypalErrorMessage"]);
						this.Logger.LogError(ex);
					}
                });

	        }
	    }

		private async Task DeleteCreditCard()
		{
			if (!IsEditing)
			{
				return;
			}

			await _accountService.RemoveCreditCard(Data.CreditCardId);

			if (_accountService.CurrentAccount.DefaultCreditCard == null)
			{
				ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser = bool.TrueString });
			}
			else
			{
				Close(this);
			}
		}

        private async Task SaveCreditCard()
        {
            try
            {
                Data.CreditCardCompany = CreditCardTypeName;

                if (!Data.CCV.HasValue() && Data.Label != _originalLabel && !_isAddingNew)
                {
                    using (this.Services().Message.ShowProgress())
                    {
                        var success = await _accountService.UpdateCreditCardLabel(Data.CreditCardId, Data.Label);

                        if (success)
                        {
                            Close(this);
                        }
                        else
                        {
                            await this.Services().Message.ShowMessage(null, this.Services().Localize["CreditCardError_Label"]);
                        }
                    }
                    return;
                }

                if (Params.Get(Data.NameOnCard,
                    Data.CardNumber,
                    Data.CreditCardCompany,
                    Data.ExpirationMonth,
                    Data.ExpirationYear,
                    Data.CCV,
                    Data.ZipCode).Any(x => x.IsNullOrEmpty()))
                {
                    await this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardRequiredFields"]);
                    return;
                }

                if (!IsValid(Data.CardNumber))
                {
                    await this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardInvalidCrediCardNUmber"]);
                    return;
                }

                using (this.Services().Message.ShowProgress())
                {
                    if (PaymentSettings.PaymentMode == PaymentMethod.Braintree)
                    {
                        await SaveBraintreePayment();
                    }
                    else
                    {
                        await SaveCreditCardGeneric();
                    }

                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex);
            }
        }


	    private async Task SaveBraintreePayment()
	    {
            var clientToken = await _paymentService.GenerateClientTokenResponse();

            var nameOnCard = Data.NameOnCard.Split(' ');

            var paymentNonce = await _paymentProviderClientService.GetCreditCardNonce(
                    clientToken.ClientToken,
                    Data.CardNumber,
                    Data.CCV,
                    Data.ExpirationMonth,
                    Data.ExpirationYear,
                    nameOnCard.FirstOrDefault(),
                    nameOnCard.LastOrDefault(),
                    Data.ZipCode
                );

	        var creditCardId = Data.CreditCardId == Guid.Empty
	            ? (Guid?)null
	            : Data.CreditCardId;

	        await _paymentService.AddPaymentMethod(paymentNonce, PaymentMethods.CreditCard, creditCardId, Data.NameOnCard);

            CloseView();
        }

	    private async Task SaveCreditCardGeneric()
	    {
	        Data.Last4Digits = new string(Data.CardNumber.Reverse().Take(4).Reverse().ToArray());

	        if (!IsEditing)
	        {
	            Data.CreditCardId = Guid.NewGuid();
	        }

	        var success = await _accountService.AddOrUpdateCreditCard(Data, _kountSessionId, IsEditing);

	        if (success)
	        {
	            _deviceCollectorService.GenerateNewSessionIdAndCollect();

	            this.Services().Analytics.LogEvent("AddCOF");
	            Data.CardNumber = null;
	            Data.CCV = null;
                        
	            if(IsMandatory && !_hasPaymentToSettle)
	            {
	                await this.Services().Message.ShowMessage(string.Empty,
	                    PaymentSettings.IsPaymentOutOfAppDisabled != OutOfAppPaymentDisabled.None
	                        ? this.Services().Localize["CreditCardAdded_PayInCarDisabled"]
	                        : this.Services().Localize["CreditCardAdded"]);
	            }

	            if(_isFromPromotionsView || _isFromCreditCardListView || _hasPaymentToSettle)
	            {
	                // We are from the promotion or mutliple credit card pages, we should return to it.
	                Close(this);
	            }
	            else
	            {
	                ShowViewModelAndClearHistory<HomeViewModel>(new { locateUser = bool.TrueString });
	            }
	        }
	        else
	        {
	            await this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardErrorInvalid"]);
	        }
	    }

	    private void CloseView()
	    {
	        if (_isFromPromotionsView || _isFromCreditCardListView)
	        {
	            // We are from the promotion or mutliple credit card pages, we should return to it.
	            Close(this);
	        }
	        else
	        {
	            ShowViewModelAndClearHistory<HomeViewModel>(new {locateUser = bool.TrueString});
	        }
	    }

		private bool IsValid(string cardNumber)
		{
			var number = new byte[16]; // number to validate

			// Remove non-digits
			var len = 0;
			for(var i = 0; i < cardNumber.Length; i++)
			{
				if (char.IsDigit (cardNumber, i))
				{
					if (len == 16)
						return false; // number has too many digits
					number[len++] = byte.Parse (cardNumber[i].ToString (CultureInfo.InvariantCulture), null);
				}
				else
				{
					return false; // non-digit char
				}
			}

			// Use Luhn Algorithm to validate
			var sum = 0;
			for(var i = len - 1; i >= 0; i--)
			{
				if (i % 2 == len % 2)
				{
					var n = number[i] * 2;
					sum += (n / 10) + (n % 10);
				}
				else
				{
					sum += number[i];
				}
			}
			return (sum % 10 == 0);
		}

		private bool HasValidDate(string month, string year)
		{
			var creditCard = new CreditCardDetails 
				{
					ExpirationMonth = month,
					ExpirationYear = year
				};

			return !creditCard.IsExpired();
		}

		private void DetermineCompany(string cardNumber)
		{
			var visaRgx = new Regex(VisaPattern, RegexOptions.IgnoreCase);
			var matches = visaRgx.Matches(cardNumber);

			if (matches.Count > 0)
			{
				if (_visaElectronFirstNumbers.Any(x => cardNumber.StartsWith(x)) &&
					cardNumber.Count() == 16)
				{
					var id = CreditCardCompanies.Find(x => x.Display == VisaElectron).Id;
					if (id != null)
					{
						CreditCardType = (int) id;
					}
				}
				else
				{
					var id = CreditCardCompanies.Find(x => x.Display == Visa).Id;
					if (id != null)
					{
						CreditCardType = (int) id;
					}
				}
			}
			else
			{
				var masterRgx = new Regex(MasterPattern, RegexOptions.IgnoreCase);
				matches = masterRgx.Matches(cardNumber);
				if (matches.Count > 0)
				{
					var id = CreditCardCompanies.Find(x=> x.Display == MasterCard).Id;
					if (id != null)
						CreditCardType = (int)id;
				}
				else
				{
					var amexRgx = new Regex(AmexPattern, RegexOptions.IgnoreCase);
					matches = amexRgx.Matches(cardNumber);
					if (matches.Count > 0)
					{
						var id = CreditCardCompanies.Find(x => x.Display == Amex).Id;
						if (id != null)
							CreditCardType = (int)id;
					}
					else
					{
						var i = CreditCardCompanies.Find(x => x.Display == CreditCardGeneric).Id;
						if (i != null)
							CreditCardType = (int)i;
					}
				}
			}
		}
	}
}