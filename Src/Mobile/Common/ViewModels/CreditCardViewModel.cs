using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Interfaces.Commands;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CreditCardViewModel : BaseViewModel
    {
        public CreditCardDetails CreditCardDetails { get; set; }
        public string Last4DigitsWithStars
        {
            get
            {
                if(!IsAddNew)
                {
                    return "\u2022\u2022\u2022\u2022 " + this.CreditCardDetails.Last4Digits;
                }
                return "";
            }
            set{
                this.Last4DigitsWithStars = value;
            }
        }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public bool ShowPlusSign { get; set; }
        public bool IsAddNew { get; set; }
        public string Picture { get; set; }
        public IMvxCommand RemoveCreditCards
        {
            get
            {
                return GetCommand(() =>
                                      {
                                          TinyIoCContainer.Current.Resolve<IAccountService>()
                                                          .RemoveCreditCard(this.CreditCardDetails.CreditCardId);
                                          this.MessengerHub.Publish(new CreditCardRemoved(this,this.CreditCardDetails.CreditCardId,null));
                                      });
            }
        }
    }
}