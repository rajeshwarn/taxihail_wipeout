using System;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Common
{
    public struct Fare
    {
        public decimal AmountInclTax { get; private set; }
        public decimal AmountExclTax { get; private set; }
        public decimal TaxRate { get; private set; }
        public decimal TaxAmount { get; private set; }

        public static Fare FromAmountInclTax(decimal amount, decimal taxRate)
        {
            decimal amountExclTax = amount / (1 + taxRate/100);
            decimal taxAmount = amount - amountExclTax;
            return new Fare
                       {
                           AmountInclTax = amount,
                           TaxRate = taxRate,
                           AmountExclTax = amountExclTax,
                           TaxAmount = taxAmount
                       };
        } 
    }
}