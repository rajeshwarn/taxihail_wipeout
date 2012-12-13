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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class CreditCardAddViewModel : BaseSubViewModel<CreditCardInfos>, IMvxServiceConsumer<IAccountService>
    {
        IAccountService _accountService;

		public CreditCardAddViewModel (string messageId) : base(messageId)
        {
            Data = new CreditCardInfos();
            _accountService = this.GetService<IAccountService>();

			CardCategories = new List<ListItem>();
			CardCategories.Add (new ListItem{ Id = 0, Display = "Personnal"} );
			CardCategories.Add (new ListItem{ Id = 1, Display = "Work"});
			CardCategories.Add (new ListItem{ Id = 2, Display = "Other"});
			CreditCardCategory = 0;
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

		public List<ListItem> CardCategories { get; set; }

		public IMvxCommand SetCreditCardCompanyCommand { get { return GetCommand<object>(item => {
					Data.CreditCardCompany = item.ToSafeString();	
		}); } }

        public IMvxCommand AddCreditCardCommand { get { return GetCommand(AddCrediCard); } }

        private void AddCrediCard ()
        {
			Data.FriendlyName = CreditCardCategoryName;
            if (Params.Get<string> (Data.NameOnCard, Data.CardNumber, 
                                   Data.CreditCardCompany, Data.FriendlyName, 
                                   Data.ExpirationMonth, 
                                   Data.ExpirationYear, 
                                   Data.CCV, 
                                   Data.ZipCode).Any (x => x.IsNullOrEmpty ())) {
				MessageService.ShowMessage(Resources.GetString("CreditCardErrorTitle"), Resources.GetString("CreditCardRequiredFields"));
				return;
            }

            if (!IsValidate (Data.CardNumber)) {
				MessageService.ShowMessage(Resources.GetString("CreditCardErrorTitle"), Resources.GetString("CreditCardInvalidCrediCardNUmber"));
				return;
            }

            try {
                MessageService.ShowProgress(true);
                Data.Last4Digits = new string(Data.CardNumber.Reverse ().Take (4).ToArray());
                _accountService.AddCreditCard (Data);

				Data.CardNumber = null;
				Data.CCV = null;

				ReturnResult(Data);
            } finally {
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


