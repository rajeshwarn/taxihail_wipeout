using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class CreditCardAddViewModel : PageViewModel
    {
		private readonly ILocationService _locationService;
		private readonly IPaymentService _paymentService;
		private readonly IAccountService _accountService;

		public CreditCardAddViewModel(ILocationService locationService,
			IPaymentService paymentService, 
			IAccountService accountService)
		{
			_locationService = locationService;
			_paymentService = paymentService;
			_accountService = accountService;
		}

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
#endregion

        public class DummyVisa
        {
			public static string BraintreeNumber = "4009 3488 8888 1881".Replace(" ", "");
            public static string CmtNumber = "4012 0000 3333 0026".Replace(" ", "");
            public static int AvcCvvCvv2 = 135;
            public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
        }

		public void Init(bool showInstructions)
		{
			ShowInstructions = showInstructions;
		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);
			// we stop the service when the viewmodel starts because it stops after the homeviewmodel starts when we press back
			// this ensures that we don't stop the service just after having started it in homeviewmodel
			_locationService.Stop();
		}

		public override async void Start()
        {
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

			var creditCard = await _accountService.GetCreditCard ();
			if (creditCard == null)
			{
				Data.NameOnCard = _accountService.CurrentAccount.Name;

				var id = CreditCardCompanies.Find(x => x.Display == CreditCardGeneric).Id;
				CreditCardType = (int)id;

#if DEBUG
				if (_paymentService.GetPaymentSettings().PaymentMode == PaymentMethod.Braintree)
				{
					CreditCardNumber = DummyVisa.BraintreeNumber;
				}
				else
				{
					CreditCardNumber = DummyVisa.CmtNumber;
				}

				Data.CCV = DummyVisa.AvcCvvCvv2+"";
				Data.ExpirationMonth = DummyVisa.ExpirationDate.Month+"";
				Data.ExpirationYear = DummyVisa.ExpirationDate.Year + "";
#endif         
			}
			else
			{
				IsEditing = true;

				Data.CreditCardId = creditCard.CreditCardId;
				Data.CardNumber = "************" + creditCard.Last4Digits;
				Data.NameOnCard = creditCard.NameOnCard;
				Data.CreditCardCompany = creditCard.CreditCardCompany;

				ExpirationMonth = string.IsNullOrWhiteSpace(creditCard.ExpirationMonth) ? 1 : int.Parse(creditCard.ExpirationMonth);
				ExpirationYear = string.IsNullOrWhiteSpace(creditCard.ExpirationYear) ? DateTime.Today.Year : int.Parse(creditCard.ExpirationYear);

				var id = CreditCardCompanies.Find(x => x.Display == creditCard.CreditCardCompany).Id;
				if (id != null)
				{
					CreditCardType = (int) id;
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
                    ? default(int?) 
                    : int.Parse(Data.ExpirationYear);
            }
            set 
			{
                var year = value;
                var current = string.IsNullOrEmpty(Data.ExpirationYear) 
					? default(int?) 
					: int.Parse(Data.ExpirationYear);

                if(year != current)
				{
                    Data.ExpirationYear = year.ToSafeString();
					RaisePropertyChanged();
                }
            }
        }

        public int? ExpirationMonth 
		{
            get 
			{
                return string.IsNullOrEmpty(Data.ExpirationMonth) 
                    ? default(int?) 
                    : int.Parse(Data.ExpirationMonth);
            }
            set 
			{
                var month = value;
                var current = string.IsNullOrEmpty(Data.ExpirationMonth) ? default(int?) : int.Parse(Data.ExpirationMonth);

                if(month != current)
				{
                    Data.ExpirationMonth = month.ToSafeString();
					RaisePropertyChanged(() => ExpirationMonth);
					RaisePropertyChanged(() => ExpirationMonthDisplay);
                }
            }
        }

        public string ExpirationMonthDisplay 
		{
            get 
			{
                var type = ExpirationMonths.FirstOrDefault(x => x.Id == ExpirationMonth);
                return type == null ? null : type.Display;
            }
        }

        public List<ListItem> CreditCardCompanies { get; set; }
        public List<ListItem> ExpirationYears { get; set; }
        public List<ListItem> ExpirationMonths { get; set; }
		public bool ShowInstructions { get; set; }

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
	            }
	        }
	    }
			
		public ICommand SaveCreditCardCommand { get { return this.GetCommand(() => SaveCreditCard()); } }
		public ICommand DeleteCreditCardCommand { get { return this.GetCommand(() => DeleteCreditCard()); } }

        private void DeleteCreditCard()
        {
            this.Services().Message.ShowMessage(
                this.Services().Localize["DeleteCreditCardTitle"],
                this.Services().Localize["DeleteCreditCard"],
                this.Services().Localize["Delete"], () =>
                {
                    _accountService.RemoveCreditCard();
                    this.Services().Cache.Clear("Account.CreditCards");
                    ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser = bool.TrueString });
                },
                this.Services().Localize["Cancel"], () => { });
        }

		private async void SaveCreditCard ()
        {
            Data.CreditCardCompany = CreditCardTypeName;
            if (Params.Get (Data.NameOnCard, Data.CardNumber, 
                                   Data.CreditCardCompany, 
                                   Data.ExpirationMonth, 
                                   Data.ExpirationYear, 
                                   Data.CCV).Any (x => x.IsNullOrEmpty ())) 
            {
                this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardRequiredFields"]);
				return;
            }

			if (!IsValid (Data.CardNumber)) 
			{
                this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardInvalidCrediCardNUmber"]);
				return;
            }

            try
            {
				var success = false;
				using(this.Services().Message.ShowProgress())
				{
	                Data.Last4Digits = new string(Data.CardNumber.Reverse().Take(4).Reverse().ToArray());
	                Data.CreditCardId = Guid.NewGuid();

					success = IsEditing 
						? await _accountService.UpdateCreditCard(Data) 
						: await _accountService.AddCreditCard(Data);
				}
				if (success)
				{	
					this.Services().Analytics.LogEvent("AddCOF");
					Data.CardNumber = null;
					Data.CCV = null;

					ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser = bool.TrueString });

					// update default card
					var account = _accountService.CurrentAccount;
					account.DefaultCreditCard = Data.CreditCardId;
					_accountService.UpdateSettings(account.Settings, Data.CreditCardId, account.DefaultTipPercent);
				}
				else
				{
                    this.Services().Message.ShowMessage("Validation", "Cannot validate the credit card.");
				}
            }
			finally 
			{
				this.Services().Cache.Clear("Account.CreditCards");
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


