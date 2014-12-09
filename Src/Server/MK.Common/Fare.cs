using System;

namespace apcurium.MK.Common
{
    public struct Fare
    {
        public double AmountInclTax { get; private set; }
        public double AmountExclTax { get; private set; }
        public double TaxRate { get; private set; }
        public double TaxAmount { get; private set; }

        public static Fare FromAmountInclTax(double amount, double taxRate)
        {
            var amountExclTax = Math.Round(amount/(1 + taxRate/100), 2);
            var taxAmount = amount - amountExclTax;

            return new Fare
            {
                AmountInclTax = amount,
                TaxRate = taxRate,
                AmountExclTax = amountExclTax,
                TaxAmount = Math.Round(taxAmount, 2)
            };
        }
    }
}