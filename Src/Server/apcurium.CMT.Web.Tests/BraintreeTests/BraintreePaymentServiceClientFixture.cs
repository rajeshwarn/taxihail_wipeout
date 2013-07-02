using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MK.Booking.Api.Client;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.BrainTree;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.CMT.Web.Tests.BraintreeTests
{
    public class BraintreePaymentServiceClientFixture :BasePaymentClientFixture
    {
        public BraintreePaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Braintree)
        {
            
        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new BraintreeServiceClient(new BraintreeSettings());
        }
    }
}
