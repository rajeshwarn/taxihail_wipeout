#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class AddCreditCard : ICommand
    {
        public AddCreditCard()
        {
            Id = Guid.NewGuid();
        }

        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; }
        public string NameOnCard { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string CreditCardCompany { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; } 
        public Guid Id { get; set; }
    }
}