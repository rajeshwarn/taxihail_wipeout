using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class CreditCardAddViewModel : PageViewModel
    {
		private readonly ILocationService _locationService;
		private readonly IPaymentService _paymentService;
		private readonly IAccountService _accountService;

		private OverduePayment _paymentToSettle;

	    public CreditCardAddViewModel(
            ILocationService locationService,
			IPaymentService paymentService, 
			IAccountService accountService)
		{
			_locationService = locationService;
			_paymentService = paymentService;
			_accountService = accountService;
		}

		private bool _isFromPromotions;
		
#region Const and ReadOnly
        private const string Visa = "Visa";
        private const string MasterCard = "MasterCard";
        private const string Amex = "Amex";
		private const string CreditCardGeneric = "Credit Card Generic";
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
        }

		public void Init(bool showInstructions = false, bool isMandatory = false, string paymentToSettle = null, bool isFromPromotions = false)
		{
			ShowInstructions = showInstructions;
			IsMandatory = isMandatory;
			
			_isFromPromotions = isFromPromotions;

			if (paymentToSettle != null)
			{
				_paymentToSettle = JsonSerializer.DeserializeFromString<OverduePayment>(paymentToSettle);
			}

		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);
			// we stop the service when the viewmodel starts because it stops after the homeviewmodel starts when we press back
			// this ensures that we don't stop the service just after having started it in homeviewmodel
			_locationService.Stop();
		}

	    private ClientPaymentSettings _paymentSettings;

		public override async void Start()
        {
			using (this.Services ().Message.ShowProgress ())
			{
			    IsPayPalAccountLinked = _accountService.CurrentAccount.IsPayPalAccountLinked;

			    try
			    {
			        _paymentSettings = await _paymentService.GetPaymentSettings();
			    }
			    catch
			    {
			        // Do nothing
			    }

			    CreditCardCompanies = new List<ListItem>
				{
					new ListItem {Display = Visa, Id = 0},
					new ListItem {Display = MasterCard, Id = 1},
					new ListItem {Display = Amex, Id = 2},
					new ListItem {Display = VisaElectron, Id = 3},
					new ListItem {Display = CreditCardGeneric, Id = 4}
				};

				ExpirationYears = new List<ListItem>();
				for (var i = 0; i <= 15; i++)
				{
					ExpirationYears.Add (new ListItem { Id = DateTime.Today.AddYears(i).Year, Display = DateTime.Today.AddYears(i).Year.ToString(CultureInfo.InvariantCulture) });
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

				Data = new CreditCardInfos ();

				CreditCardDetails creditCard = null;

                try
                {
                    creditCard = await _accountService.GetCreditCard();
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(ex.Message, ex.ToString());
                    this.Services().Message.ShowMessage(this.Services().Localize["Error"], this.Services().Localize["PaymentLoadError"]);
                }

				if (creditCard == null)
				{
					Data.NameOnCard = _accountService.CurrentAccount.Name;

					var id = CreditCardCompanies.Find(x => x.Display == CreditCardGeneric).Id;
					CreditCardType = (int)id;

					#if DEBUG
					if (_paymentSettings.PaymentMode == PaymentMethod.Braintree)
					{
						CreditCardNumber = DummyVisa.BraintreeNumber;
					}
					else
					{
						CreditCardNumber = DummyVisa.CmtNumber;
					}

					Data.CCV = DummyVisa.AvcCvvCvv2+"";

					ExpirationMonth = DummyVisa.ExpirationDate.Month;
					ExpirationYear = DummyVisa.ExpirationDate.Year;
					#endif         
				}
				else
				{
					IsEditing = true;

					Data.CreditCardId = creditCard.CreditCardId;
					Data.CardNumber = "************" + creditCard.Last4Digits;
					Data.NameOnCard = creditCard.NameOnCard;
					Data.CreditCardCompany = creditCard.CreditCardCompany;

    				ExpirationMonth = string.IsNullOrWhiteSpace(creditCard.ExpirationMonth) ? (int?)null : int.Parse(creditCard.ExpirationMonth);
    				ExpirationYear = string.IsNullOrWhiteSpace(creditCard.ExpirationYear) ? (int?)null : int.Parse(creditCard.ExpirationYear);

					var id = CreditCardCompanies.Find(x => x.Display == creditCard.CreditCardCompany).Id;
					if (id != null)
					{
						CreditCardType = (int) id;
					}
				}

				RaisePropertyChanged(() => Data);
				RaisePropertyChanged(() => CreditCardNumber);
                RaisePropertyChanged(() => CanDeleteCreditCard);
                RaisePropertyChanged(() => IsPayPalOnly);

                if (_paymentToSettle != null)
                {
                    return;
                }

                try
                {
                    var overduePayment = await _paymentService.GetOverduePayment();

                    if (overduePayment == null)
                    {
                        return;
                    }

                    this.Services().Message.ShowMessage(
                        this.Services().Localize["View_Overdue"],
                        this.Services().Localize["Overdue_OutstandingPaymentExists"],
                        this.Services().Localize["OkButtonText"],
                        () => ShowViewModelAndRemoveFromHistory<OverduePaymentViewModel>(new
                        {
                            overduePayment = overduePayment.ToJson()
                        }),
                        this.Services().Localize["Cancel"],
                        () => Close(this));
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
			}
        }

	    private bool _isPayPalAccountLinked;
	    public bool IsPayPalAccountLinked
	    {
            get { return _isPayPalAccountLinked; }
	        set
	        {
	            if (_isPayPalAccountLinked != value)
	            {
	                _isPayPalAccountLinked = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(() => CanLinkPayPalAccount);
                    RaisePropertyChanged(() => CanUnlinkPayPalAccount);
                    RaisePropertyChanged(() => ShowLinkedPayPalInfo);
	            }
	        }
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
				Data.ExpirationYear = value.ToSafeString();
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
				Data.ExpirationMonth = value.ToSafeString();

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
            get { return IsEditing && !Settings.CreditCardIsMandatory; }
        }

        public bool CanLinkPayPalAccount
        {
            get
            {
                return !IsPayPalAccountLinked
                    && _paymentSettings != null
                    && _paymentSettings.PayPalClientSettings.IsEnabled;
            }
        }

	    public bool CanUnlinkPayPalAccount
	    {
            get { return IsPayPalAccountLinked && !Settings.CreditCardIsMandatory; }
	    }

	    public bool ShowLinkedPayPalInfo
	    {
            get { return IsPayPalAccountLinked && Settings.CreditCardIsMandatory; }
	    }

	    public bool IsPayPalOnly
	    {
	        get
	        {
	            return _paymentSettings != null
                    && _paymentSettings.PayPalClientSettings.IsEnabled
                    && !_paymentSettings.IsPayInTaxiEnabled;
	        }
	    }

		public string CreditCardSaveButtonDisplay
		{
			get
            {
				return IsEditing ? this.Services().Localize["Modify"] : this.Services().Localize["Save"];
			}
		}

		public bool ShouldDisplayTip
		{
			get
			{
				return _paymentSettings.IsPayInTaxiEnabled || _paymentSettings.PayPalClientSettings.IsEnabled;
			}
		}

		private PaymentDetailsViewModel _paymentPreferences;
		public PaymentDetailsViewModel PaymentPreferences
		{
			get
			{
				if (_paymentPreferences == null)
				{
					_paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
					_paymentPreferences.Start();
					_paymentPreferences.ActionOnTipSelected = _saveTip;
				}
				return _paymentPreferences;
			}
		}

		private ICommand _saveTip 
		{ 
			get
			{
				return this.GetCommand<int>(async tip =>
					{
                        if (PaymentPreferences.Tip > TipMaxPercent)
                        {
                            await this.Services().Message.ShowMessage(null, this.Services().Localize["TipPercent_Error"]);
                            return;
                        }

                        try
                        {
                            await _accountService.UpdateSettings(_accountService.CurrentAccount.Settings, PaymentPreferences.Tip);
                        }
                        catch (WebServiceException)
                        {
                            this.Services()
                                .Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
                                    this.Services().Localize["UpdateBookingSettingsGenericError"]);
                        }
					});
			} 
		} 

		public ICommand SaveCreditCardCommand 
        { 
            get
            {
                return this.GetCommand(() =>
		        {
                    if (IsPayPalAccountLinked)
                    {
                        this.Services().Message.ShowMessage(
                            this.Services().Localize["AddCreditCardTitle"],
                            this.Services().Localize["AddCoFPayPalWarning"],
                            this.Services().Localize["AddACardButton"], SaveCreditCard,
                            this.Services().Localize["Cancel"], () => { });
                    }
                    else
                    {
                        SaveCreditCard();
                    }
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

        public async void LinkPayPalAccount(string authCode)
        {
            try
            {
				await _accountService.LinkPayPalAccount(authCode);

                IsPayPalAccountLinked = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                this.Services().Message.ShowMessage(
                    this.Services().Localize["PayPalErrorTitle"],
                    this.Services().Localize["PayPalLinkError"]);
                return;
            }

            try
            {
                await DeleteCreditCard(true);

                this.Services().Message.ShowMessage(
                    string.Empty,
                    this.Services().Localize["PayPalLinked"],
                    () => ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser = bool.TrueString }));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                this.Services().Message.ShowMessage(
                    this.Services().Localize["PayPalErrorTitle"],
                    this.Services().Localize["PayPalLinkError"]);

                UnlinkPayPalAccount();
            }
        }

		public void UnlinkPayPalAccount(bool replacedByCreditCard = false)
        {
            if (!IsPayPalAccountLinked)
            {
                return;
            }

            try
            {
				_accountService.UnlinkPayPalAccount(replacedByCreditCard);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                this.Services().Message.ShowMessage(
                    this.Services().Localize["PayPalErrorTitle"],
                    this.Services().Localize["PayPalUnlinkError"]);
            }

			IsPayPalAccountLinked = false;
        }

		private async Task DeleteCreditCard(bool replacedByPayPal = false)
	    {
	        if (!IsEditing)
	        {
	            return;
	        }

			await _accountService.RemoveCreditCard(replacedByPayPal);
            
			if (!replacedByPayPal)
			{
				ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser = bool.TrueString });
			}
	    }

        private async void SaveCreditCard()
	    {
			try
			{
				Data.CreditCardCompany = CreditCardTypeName;

				if (Params.Get(Data.NameOnCard, 
					Data.CardNumber,
					Data.CreditCardCompany,
					Data.ExpirationMonth,
					Data.ExpirationYear,
					Data.CCV).Any(x => x.IsNullOrEmpty()))
				{
					await this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardRequiredFields"]);
					return;
				}

				if (!IsValid(Data.CardNumber))
				{
					await this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardInvalidCrediCardNUmber"]);
					return;
				}

				if(!HasValidDate(Data.ExpirationMonth, Data.ExpirationYear))
				{
					await this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardErrorInvalid"]);
					return;
				}

				using (this.Services().Message.ShowProgress())
				{
					Data.Last4Digits = new string(Data.CardNumber.Reverse().Take(4).Reverse().ToArray());

					if (!IsEditing)
					{
						Data.CreditCardId = Guid.NewGuid();
					}

					var success = await _accountService.AddOrUpdateCreditCard(Data, IsEditing);

					if (success)
					{
						UnlinkPayPalAccount(true);

						this.Services().Analytics.LogEvent("AddCOF");
						Data.CardNumber = null;
						Data.CCV = null;

                        if (_paymentToSettle != null)
					    {
                            await SettleOverduePayment();
					    }
					    else
					    {
                            await this.Services().Message.ShowMessage(string.Empty, 
                                _paymentSettings.IsOutOfAppPaymentDisabled ? 
                                this.Services().Localize["CreditCardAdded_PayInCarDisabled"] :
                                this.Services().Localize["CreditCardAdded"]);
					    }
						
						if(_isFromPromotions)
						{
							// We are from the promotion page, we should return to it.
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
			}
			catch(Exception ex)
			{
				this.Logger.LogError(ex);
			}
	    }

	    private async Task SettleOverduePayment()
	    {
            var settleOverduePayment = await _paymentService.SettleOverduePayment();

	        if (settleOverduePayment.IsSuccessful)
	        {
                var message = string.Format(this.Services().Localize["Overdue_Succeed_Message"], _paymentToSettle.OverdueAmount);
                await this.Services().Message.ShowMessage(this.Services().Localize["Overdue_Succeed_Title"], message);
	        }
	        else
	        {
                await this.Services().Message.ShowMessage(this.Services().Localize["Overdue_Failed_Title"], this.Services().Localize["Overdue_Failed_Message"]);
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
                if(i % 2 == len % 2)
                {
                    var n = number[i] * 2;
                    sum += (n / 10) + (n % 10);
                }
                else
                    sum += number[i];
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
