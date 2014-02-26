using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class CreditCardAddViewModel : BaseSubViewModel<CreditCardInfos>
    {
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

		public override void Start()
        {
            Data = new CreditCardInfos();

			CardCategories = new List<ListItem>
			{
			    new ListItem {Id = 0, Display = "Personal"},
			    new ListItem {Id = 1, Display = "Work"},
			    new ListItem {Id = 2, Display = "Other"}
			};
		    CreditCardCategory = 0;

            CreditCardCompanies = new List<ListItem>
            {
				new ListItem {Display = Visa, Id = 0},
                new ListItem {Display = MasterCard, Id = 1},
                new ListItem {Display = Amex, Id = 2},
                new ListItem {Display = VisaElectron, Id = 3},
                new ListItem {Display = CreditCardGeneric, Id = 4}
            };

		    var id = CreditCardCompanies.Find(x => x.Display == CreditCardGeneric).Id;
		    if (id != null)
		    {
		        CreditCardType = (int)id;

		        ExpirationYears = new List<ListItem>();
		        for (int i = 0; i <= 15; i++)
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
		    }

#if DEBUG
			if (this.Services().Payment.GetPaymentSettings().PaymentMode == PaymentMethod.Braintree)
			{
				CreditCardNumber = DummyVisa.BraintreeNumber;
			}
			else{
				CreditCardNumber = DummyVisa.CmtNumber;
			}
			Data.CCV = DummyVisa.AvcCvvCvv2+"";

			Data.ExpirationMonth = DummyVisa.ExpirationDate.Month+"";
			Data.ExpirationYear = DummyVisa.ExpirationDate.Year + "";
			Data.NameOnCard = "Chris";
			RaisePropertyChanged("CreditCardNumber");
#endif            
        }

#region Properties
        public CreditCardInfos Data { get; set; }


        //todo: refactorer le setter
        public string CreditCardNumber
        {
            get{ return Data.CardNumber;}
            set
            {
                Data.CardNumber = value;

                Regex visaRgx = new Regex(VisaPattern, RegexOptions.IgnoreCase);
                MatchCollection matches = visaRgx.Matches(Data.CardNumber);

                if (matches.Count > 0)
                {
                    if (_visaElectronFirstNumbers.Any(x => Data.CardNumber.StartsWith(x)) &&
                        Data.CardNumber.Count() == 16)
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

                    Regex masterRgx = new Regex(MasterPattern, RegexOptions.IgnoreCase);
                    matches = masterRgx.Matches(Data.CardNumber);
                    if (matches.Count > 0)
                    {
                        var id = CreditCardCompanies.Find(x=> x.Display == MasterCard).Id;
                        if (id != null)
                            CreditCardType = (int)id;
                    }
                    else
                    {
                        Regex amexRgx = new Regex(AmexPattern, RegexOptions.IgnoreCase);
                        matches = amexRgx.Matches(Data.CardNumber);
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
				RaisePropertyChanged();
            }
        }

		int _creditCardCategory;
		public int CreditCardCategory {
			get {
				return _creditCardCategory;
			}
			set {
				_creditCardCategory = value;
				RaisePropertyChanged();
				RaisePropertyChanged("CreditCardCategoryName");
			}
		}

		public string CreditCardCategoryName { 
			get {
				var category = CardCategories.FirstOrDefault(x=>x.Id == CreditCardCategory);
				if(category == null) return null;
				return category.Display; 
			}
		}

        int _creditCardType;
        public int CreditCardType {
            get {return _creditCardType;}
            set {
                _creditCardType = value;
				RaisePropertyChanged();
				RaisePropertyChanged("CreditCardTypeName");
				RaisePropertyChanged("CreditCardImagePath");
            }
        }

        public string CreditCardTypeName { 
            get {
                var type = CreditCardCompanies.FirstOrDefault(x=>x.Id == CreditCardType);
				return type == null ? null : type.Display;
            }
        }

        public string CreditCardImagePath { 
            get {
                var type = CreditCardCompanies.FirstOrDefault(x=>x.Id == CreditCardType);
				return type == null ? null : type.Image;
            }
        }

        public int? ExpirationYear {
            get {
                return string.IsNullOrEmpty(Data.ExpirationYear) 
                    ? default(int?) 
                    : int.Parse(Data.ExpirationYear);
            }
            set {
                var year = value;
                var current = string.IsNullOrEmpty(Data.ExpirationYear) ? default(int?) : int.Parse(Data.ExpirationYear);

                if(year != current){
                    Data.ExpirationYear = year.ToSafeString();
					RaisePropertyChanged();
                }
            }
        }

        public int? ExpirationMonth {
            get {
                return string.IsNullOrEmpty(Data.ExpirationMonth) 
                    ? default(int?) 
                    : int.Parse(Data.ExpirationMonth);
            }
            set {
                var month = value;
                var current = string.IsNullOrEmpty(Data.ExpirationMonth) ? default(int?) : int.Parse(Data.ExpirationMonth);

                if(month != current){
                    Data.ExpirationMonth = month.ToSafeString();
					RaisePropertyChanged("ExpirationMonth");
					RaisePropertyChanged("ExpirationMonthDisplay");
                }
            }
        }

        public string ExpirationMonthDisplay {
            get {
                var type = ExpirationMonths.FirstOrDefault(x => x.Id == ExpirationMonth);
                return type == null ? null : type.Display;
            }
        }

		public List<ListItem> CardCategories { get; set; }

        public List<ListItem> CreditCardCompanies { get; set; }

        public List<ListItem> ExpirationYears { get; set; }

        public List<ListItem> ExpirationMonths { get; set; }

		public ICommand SetCreditCardCompanyCommand
        {
            get { return this.GetCommand<object>(item => {
					Data.CreditCardCompany = item.ToSafeString();	
		}); } }

		public ICommand AddCreditCardCommand { get { return this.GetCommand(() => AddCrediCard()); } }

		public bool ShowInstructions { get; set; }

#endregion

#region Private Methods
		private async void AddCrediCard ()
        {
			Data.FriendlyName = CreditCardCategoryName;
            Data.CreditCardCompany = CreditCardTypeName;
            if (Params.Get (Data.NameOnCard, Data.CardNumber, 
                                   Data.CreditCardCompany, Data.FriendlyName, 
                                   Data.ExpirationMonth, 
                                   Data.ExpirationYear, 
                                   Data.CCV).Any (x => x.IsNullOrEmpty ())) 
            {
                this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardRequiredFields"]);
				return;
            }

            if (!IsValidate (Data.CardNumber)) {
                this.Services().Message.ShowMessage(this.Services().Localize["CreditCardErrorTitle"], this.Services().Localize["CreditCardInvalidCrediCardNUmber"]);
				return;
            }

            try
            {
                this.Services().Message.ShowProgress(true);
                Data.Last4Digits = new string(Data.CardNumber.Reverse().Take(4).Reverse().ToArray());
                Data.CreditCardId = Guid.NewGuid();

				var success = await this.Services().Account.AddCreditCard(Data);
				if (success)
				{		
					Data.CardNumber = null;
					Data.CCV = null;

					if(ShowInstructions)
					{
						ShowViewModelAndRemoveFromHistory<HomeViewModel>();
					}
					else
					{
						ReturnResult(Data);
					}
				}
				else
				{
                    this.Services().Message.ShowMessage("Validation", "Cannot validate the credit card.");
				}

            }
            finally {
                this.Services().Cache.Clear("Account.CreditCards");
                this.Services().Message.ShowProgress(false);
            }
        }

        private bool IsValidate(string cardNumber)
        {
            byte[] number = new byte[16]; // number to validate
            
            // Remove non-digits
            int len = 0;
            for(int i = 0; i < cardNumber.Length; i++)
            {
                if(char.IsDigit(cardNumber, i))
                {
                    if(len == 16) return false; // number has too many digits
                    number[len++] = byte.Parse(cardNumber[i].ToString(CultureInfo.InvariantCulture), null);
                }
            }

            // Use Luhn Algorithm to validate
            int sum = 0;
            for(int i = len - 1; i >= 0; i--)
            {
                if(i % 2 == len % 2)
                {
                    int n = number[i] * 2;
                    sum += (n / 10) + (n % 10);
                }
                else
                    sum += number[i];
            }
            return (sum % 10 == 0);
        }

#endregion

    }
}


