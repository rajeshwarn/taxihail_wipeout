﻿using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    /// <summary>
    /// Used only for migration between older versions
    /// It helps to determine if the current account in cache is old and needs to be updated from server
    /// </summary>
    public class DeprecatedAccount 
    {
        public Guid? DefaultCreditCard { get; set; }
    }

    public class Account : BaseDto
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public int IbsAccountid { get; set; }

        public string FacebookId { get; set; }

        public string TwitterId { get; set; }

        public BookingSettings Settings { get; set; }

        public string Language { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsSuperAdmin { get; set; }

        public CreditCardDetails DefaultCreditCard { get; set; }

        public int? DefaultTipPercent { get; set; }

        public bool IsPayPalAccountLinked { get; set; }

        public bool HasValidPaymentInformation
        {
            get
            {
                return IsPayPalAccountLinked || DefaultCreditCard != null;
            }
        }
    }
}