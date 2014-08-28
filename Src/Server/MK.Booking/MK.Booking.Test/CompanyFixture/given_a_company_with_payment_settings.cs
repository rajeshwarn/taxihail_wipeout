#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_company_with_payment_settings
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Company>();
            _sut.Setup(new CompanyCommandHandler(_sut.Repository, null));
            _sut.Given(new CompanyCreated {SourceId = _companyId});
            _sut.Given(new PaymentSettingUpdated
            {
                SourceId = _companyId,
                ServerPaymentSettings = new ServerPaymentSettings
                {
                    PaymentMode = PaymentMethod.Cmt
                }
            });
        }

        private EventSourcingTestHelper<Company> _sut;
        private readonly Guid _companyId = AppConstants.CompanyId;

        [Test]
        public void when_paymentmode_changed()
        {
            var newSettings = new ServerPaymentSettings
            {
                PaymentMode = PaymentMethod.Braintree
            };

            _sut.When(new UpdatePaymentSettings
            {
                ServerPaymentSettings = newSettings
            });


            Assert.AreEqual(2, _sut.Events.Count);
            var evt = _sut.ThenHasOne<PaymentSettingUpdated>();
            var evt2 = _sut.ThenHasOne<PaymentModeChanged>();

            Assert.AreEqual(_companyId, evt2.SourceId);
        }

        [Test]
        public void when_paymentmode_changed_from_cmt_to_ridelinq()
        {
            var newSettings = new ServerPaymentSettings
            {
                PaymentMode = PaymentMethod.RideLinqCmt
            };

            _sut.When(new UpdatePaymentSettings
            {
                ServerPaymentSettings = newSettings
            });


            Assert.AreEqual(1, _sut.Events.Count);
            var evt = _sut.ThenHasOne<PaymentSettingUpdated>();
            _sut.ThenHasNo<PaymentModeChanged>();

            Assert.AreEqual(_companyId, evt.SourceId);
        }

        [Test]
        public void when_paymentsettings_updated_successfully()
        {
            var key = Guid.NewGuid().ToString();
            var newSettings = new ServerPaymentSettings
            {
                PaymentMode = PaymentMethod.Cmt,
                BraintreeClientSettings =
                {
                    ClientKey = key
                }
            };


            _sut.When(new UpdatePaymentSettings
            {
                ServerPaymentSettings = newSettings
            });


            var evt = _sut.ThenHasSingle<PaymentSettingUpdated>();
            _sut.ThenHasNo<PaymentModeChanged>(); //dont delete cc if payment mode doesnt change

            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(key, evt.ServerPaymentSettings.BraintreeClientSettings.ClientKey);

            Assert.AreSame(newSettings, evt.ServerPaymentSettings);
        }
    }
}