using System;

namespace apcurium.MK.Common
{
    public struct Fare
    {
        public decimal AmountInclTax { get; internal set; }
        public decimal AmountExclTax { get; internal set; }
        public decimal TaxRate { get; internal set; }
        public decimal TaxAmount { get; internal set; }
    }

    public static class FareHelper
    {
        public static Fare GetFareFromAmountInclTax(double amountIncludingTax, double taxRate)
        {
            return GetFareFromAmountInclTax(Convert.ToDecimal(amountIncludingTax), Convert.ToDecimal(taxRate));
        }

        public static Fare GetFareFromAmountInclTax(decimal amountIncludingTax, decimal taxRate)
        {
            var amountExclTax = Math.Round(amountIncludingTax / (1 + taxRate / 100), 2);
            var taxAmount = amountIncludingTax - amountExclTax;

            return new Fare
            {
                AmountInclTax = amountIncludingTax,
                TaxRate = taxRate,
                AmountExclTax = amountExclTax,
                TaxAmount = Math.Round(taxAmount, 2)
            };
        }

        public static decimal GetTipAmountFromTotalAmount(decimal totalAmount, decimal tipPercentage)
        {
            var tip = tipPercentage / 100;
            var amountWithoutTip = Math.Round(totalAmount/(1 + tip), 2);
            return totalAmount - amountWithoutTip;
        }

        public static double CalculateTipAmount(double amount, double tipPercentage)
        {
            return (double) CalculateTipAmount(Convert.ToDecimal(amount), Convert.ToDecimal(tipPercentage));
        }

        public static decimal CalculateTipAmount(decimal amount, decimal tipPercentage)
        {
            var tip = tipPercentage / 100;
            return Math.Round(amount * tip, 2);
        }
    }
}