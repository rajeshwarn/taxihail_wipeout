using System;
using System.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;

using apcurium.MK.Common;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class CreditCardAddViewModel : BaseSubViewModel<CreditCardInfos>, IMvxServiceConsumer<IAccountService>
    {
        IAccountService _accountService;

        public class DummyVisa
        {
			public static string BraintreeNumber = "4009 3488 8888 1881".Replace(" ", "");

            public static string CmtNumber = "4012 0000 3333 0026".Replace(" ", "");
            public static int AvcCvvCvv2 = 135;
            public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
        }

		public CreditCardAddViewModel (string messageId) : base(messageId)
        {
            Data = new CreditCardInfos();
            _accountService = this.GetService<IAccountService>();

			CardCategories = new List<ListItem>();
			CardCategories.Add (new ListItem{ Id = 0, Display = "Personal"} );
			CardCategories.Add (new ListItem{ Id = 1, Display = "Work"});
			CardCategories.Add (new ListItem{ Id = 2, Display = "Other"});
			CreditCardCategory = 0;

            CreditCardCompanies = new List<ListItem>();
            CreditCardCompanies.Add (new ListItem { Display = "Visa", Id = 0 });
            CreditCardCompanies.Add ( new ListItem { Display = "MasterCard", Id = 1 });
            CreditCardCompanies.Add ( new ListItem { Display = "Amex", Id = 2 });
            CreditCardCompanies.Add ( new ListItem { Display = "Visa Electron", Id = 3 });
            CreditCardType = 0;

            ExpirationYears = new List<ListItem>();
            for (int i = 0; i <= 15; i++)
            {
                ExpirationYears.Add (new ListItem { Id = DateTime.Today.AddYears(i).Year, Display = DateTime.Today.AddYears(i).Year.ToString() });
            }

            ExpirationMonths = new List<ListItem>();
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("January"), Id = 1 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("February"), Id = 2 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("March"), Id = 3 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("April"), Id = 4 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("May"), Id = 5 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("June"), Id = 6 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("July"), Id = 7 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("August"), Id = 8 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("September"), Id = 9 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("October"), Id = 10 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("November"), Id = 11 });
            ExpirationMonths.Add (new ListItem { Display = Resources.GetString("December"), Id = 12 });

#if DEBUG
			if(ConfigurationManager.GetPaymentSettings().PaymentMode == apcurium.MK.Common.Configuration.Impl.PaymentMethod.Braintree)
			{
				Data.CardNumber = DummyVisa.BraintreeNumber;
			}
			else{
				Data.CardNumber = DummyVisa.CmtNumber;
			}
			Data.CCV = DummyVisa.AvcCvvCvv2+"";

			Data.ExpirationMonth = DummyVisa.ExpirationDate.Month+"";
			Data.ExpirationYear = DummyVisa.ExpirationDate.Year + "";
			Data.NameOnCard = "Chris";
#endif            
        }

        public CreditCardInfos Data { get; set; }

		int _creditCardCategory;
		public int CreditCardCategory {
			get {
				return _creditCardCategory;
			}
			set {
				_creditCardCategory = value;
				FirePropertyChanged("CreditCardCategory");
				FirePropertyChanged("CreditCardCategoryName");
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
            get {
                return _creditCardType;
            }
            set {
                _creditCardType = value;
                FirePropertyChanged("CreditCardType");
                FirePropertyChanged("CreditCardTypeName");
                FirePropertyChanged("CreditCardImagePath");
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
                    FirePropertyChanged("ExpirationYear");
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
                    FirePropertyChanged("ExpirationMonth");
                    FirePropertyChanged("ExpirationMonthDisplay");
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

		public IMvxCommand SetCreditCardCompanyCommand { get { return GetCommand<object>(item => {
					Data.CreditCardCompany = item.ToSafeString();	
		}); } }

        public IMvxCommand AddCreditCardCommand { get { return GetCommand(AddCrediCard); } }

        private void AddCrediCard ()
        {
			Data.FriendlyName = CreditCardCategoryName;
            Data.CreditCardCompany = CreditCardTypeName;
            if (Params.Get (Data.NameOnCard, Data.CardNumber, 
                                   Data.CreditCardCompany, Data.FriendlyName, 
                                   Data.ExpirationMonth, 
                                   Data.ExpirationYear, 
                                   Data.CCV).Any (x => x.IsNullOrEmpty ())) 
            {
				MessageService.ShowMessage(Resources.GetString("CreditCardErrorTitle"), Resources.GetString("CreditCardRequiredFields"));
				return;
            }

            if (!IsValidate (Data.CardNumber)) {
				MessageService.ShowMessage(Resources.GetString("CreditCardErrorTitle"), Resources.GetString("CreditCardInvalidCrediCardNUmber"));
				return;
            }

            try
            {
                MessageService.ShowProgress(true);
                Data.Last4Digits = new string(Data.CardNumber.Reverse().Take(4).Reverse().ToArray());
                Data.CreditCardId = Guid.NewGuid();
                if(_accountService.AddCreditCard(Data))
				{		

					Data.CardNumber = null;
					Data.CCV = null;

					ReturnResult(Data);
				}
				else{					
					MessageService.ShowMessage ( "Validation", "Cannot validate the credit card.");
				}

            }
            finally {
                TinyIoCContainer.Current.Resolve<ICacheService>().Clear("Account.CreditCards");
                MessageService.ShowProgress(false);
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
                    number[len++] = byte.Parse(cardNumber[i].ToString(), null);
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

    }
}


