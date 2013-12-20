using System;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_company_with_payment_settings
    {
        private EventSourcingTestHelper<Company> sut;
        readonly Guid _companyId = AppConstants.CompanyId;

        [SetUp]
        public void Setup()
        {
            sut = new EventSourcingTestHelper<Company>();
            sut.Setup(new CompanyCommandHandler(this.sut.Repository));
            sut.Given(new CompanyCreated { SourceId = _companyId });
            sut.Given(new PaymentSettingUpdated()
            {
                SourceId = _companyId,
                ServerPaymentSettings = new ServerPaymentSettings()
                {
                    PaymentMode = PaymentMethod.Cmt
                }
            });
        }

        [Test]
        public void when_paymentmode_changed()
        {
            var newSettings = new ServerPaymentSettings()
            {
                PaymentMode = PaymentMethod.Braintree
            };

            sut.When(new UpdatePaymentSettings()
            {
                ServerPaymentSettings = newSettings
            });


            Assert.AreEqual(2, sut.Events.Count);
            var evt = sut.ThenHasOne<PaymentSettingUpdated>();
            var evt2 = sut.ThenHasOne<PaymentModeChanged>();

            Assert.AreEqual(_companyId, evt2.SourceId);
        }

        [Test]
        public void when_paymentmode_changed_from_cmt_to_ridelinq()
        {
            var newSettings = new ServerPaymentSettings()
            {
                PaymentMode = PaymentMethod.RideLinqCmt
            };

            sut.When(new UpdatePaymentSettings()
            {
                ServerPaymentSettings = newSettings
            });


            Assert.AreEqual(1, sut.Events.Count);
            var evt = sut.ThenHasOne<PaymentSettingUpdated>();
            sut.ThenHasNo<PaymentModeChanged>();

            Assert.AreEqual(_companyId, evt.SourceId);
        }

        [Test]
        public void when_paymentsettings_updated_successfully()
        {
            var key = Guid.NewGuid().ToString();
            var newSettings = new ServerPaymentSettings()
                                  {
                                      PaymentMode = PaymentMethod.Cmt,
                                      BraintreeClientSettings =
                                          {
                                              ClientKey = key
                                          }
                                  };


            sut.When(new UpdatePaymentSettings()
            {
                ServerPaymentSettings = newSettings
            });


            var evt = sut.ThenHasSingle<PaymentSettingUpdated>();
            sut.ThenHasNo<PaymentModeChanged>();//dont delete cc if payment mode doesnt change

            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(key, evt.ServerPaymentSettings.BraintreeClientSettings.ClientKey);

            Assert.AreSame(newSettings, evt.ServerPaymentSettings);
        }
    }
}
