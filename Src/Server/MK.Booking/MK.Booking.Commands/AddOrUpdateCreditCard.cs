﻿using System;
using apcurium.MK.Common;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddOrUpdateCreditCard : ICommand
    {
        public AddOrUpdateCreditCard()
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
        public string Label { get; set; }
        public string ZipCode { get; set; }

        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public CountryISOCode Country { get; set; }
    }
}