﻿#region

using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class CreditCardDetails
    {
        [Key]
        public Guid CreditCardId { get; set; }

        public Guid AccountId { get; set; }

        public string NameOnCard { get; set; }

        public string Token { get; set; }

        public string Last4Digits { get; set; }

        public string CreditCardCompany { get; set; }

        public string ExpirationMonth { get; set; }

        public string ExpirationYear { get; set; }

        public bool IsDeactivated { get; set; }

        public bool IsExpired()
        {
            if (!ExpirationMonth.HasValue() || !ExpirationYear.HasValue())
            {
                return false; // Prevent expiration verification from failing
            }

            var expYear = int.Parse(ExpirationYear);
            var expMonth = int.Parse(ExpirationMonth);
            var expirationDate = new DateTime(expYear, expMonth, DateTime.DaysInMonth(expYear, expMonth));

            if (expirationDate < DateTime.Now)
            {
                return true;
            }

            return false;
        }
    }
}