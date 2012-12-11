using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddCreditCard : ICommand
    {
        public AddCreditCard()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; }
        public string FriendlyName { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string CreditCardCompany { get; set; }
    }
}