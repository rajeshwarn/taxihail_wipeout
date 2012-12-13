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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CreditCardAddViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>
    {
        IAccountService _accountService;

        public CreditCardAddViewModel ()
        {
            Data = new CreditCardInfos();
            _accountService = this.GetService<IAccountService>();
        }

        public CreditCardInfos Data { get; set; }

        public IMvxCommand AddCreditCardCommand { get { return GetCommand(AddCrediCard); } }

        private void AddCrediCard ()
        {
            if (Params.Get<string> (Data.NameOnCard, Data.CardNumber, 
                                   Data.CreditCardCompany, Data.FriendlyName, 
                                   Data.ExpirationMonth, 
                                   Data.ExpirationYear, 
                                   Data.CCV, 
                                   Data.ZipCode).Any (x => x.IsNullOrEmpty ())) {
                //missing field
            }

            if (!IsValidate (Data.CardNumber)) {
                //invalid cc number            
            }

            try {
                MessageService.ShowProgress(true);
                Data.Last4Digits = new string(Data.CardNumber.Reverse ().Take (4).ToArray());
                _accountService.AddCreditCard (Data);
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


