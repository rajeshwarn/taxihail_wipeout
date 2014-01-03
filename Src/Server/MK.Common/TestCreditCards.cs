#region

using System;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Common
{
    public class TestCreditCards
    {
        public enum TestCreditCardSetting
        {
            Cmt,
            Braintree
        }

        public string[] BraintreeAmericanExpressNumbers =
        {
            "378282246310005",
            "371449635398431"
        };


        public string[] BraintreeDiscoverNumbers =
        {
            "6011111111111117"
        };

        public string[] BraintreeMasterCardNumbers =
        {
            "5555555555554444"
        };

        public string[] BraintreeVisaNumbers =
        {
            "4111111111111111",
            "4005519200000004",
            "4009348888881881",
            "4012000033330026",
            "4012000077777777",
            "4012888888881881",
            "4217651111111119",
            "4500600000000061"
        };

        public string[] CmtAmericanExpressNumbers =
        {
            "3410 9293 659 1002".Replace(" ", "")
        };

        public string[] CmtDiscoverNumbers =
        {
            "6011 0002 5950 5851".Replace(" ", "")
        };

        public string[] CmtMasterCardNumbers =
        {
            "5424 1802 7979 1732".Replace(" ", "")
        };

        public string[] CmtVisaNumbers =
        {
            "4012 0000 3333 0026".Replace(" ", "")
        };

        public TestCreditCards(TestCreditCardSetting settings)
        {
            TypeOfCardsToFurnish = settings;
        }

        public static TestCreditCardSetting TypeOfCardsToFurnish { get; set; }

        public CreditCard Visa
        {
            get
            {
                switch (TypeOfCardsToFurnish)
                {
                    case TestCreditCardSetting.Braintree:
                        return new CreditCard(BraintreeVisaNumbers.GetRandom());
                    case TestCreditCardSetting.Cmt:
                        return new CreditCard(CmtVisaNumbers.GetRandom());
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public CreditCard Mastercard
        {
            get
            {
                switch (TypeOfCardsToFurnish)
                {
                    case TestCreditCardSetting.Braintree:
                        return new CreditCard(BraintreeMasterCardNumbers.GetRandom());
                    case TestCreditCardSetting.Cmt:
                        return new CreditCard(CmtMasterCardNumbers.GetRandom());
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public CreditCard AmericanExpress
        {
            get
            {
                switch (TypeOfCardsToFurnish)
                {
                    case TestCreditCardSetting.Braintree:
                        return new CreditCard(BraintreeAmericanExpressNumbers.GetRandom(), 1234);
                    case TestCreditCardSetting.Cmt:
                        return new CreditCard(CmtAmericanExpressNumbers.GetRandom(), 1234);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public CreditCard Discover
        {
            get
            {
                switch (TypeOfCardsToFurnish)
                {
                    case TestCreditCardSetting.Braintree:
                        return new CreditCard(BraintreeDiscoverNumbers.GetRandom());
                    case TestCreditCardSetting.Cmt:
                        return new CreditCard(CmtDiscoverNumbers.GetRandom());
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}