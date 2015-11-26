using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Projections;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PaymentSettingGenerator
        : IEventHandler<PaymentSettingUpdated>
    {
        private readonly IProjectionSet<ServerPaymentSettings, string> _networkCompanyPaymentSettingsProjections;
        private readonly IProjection<ServerPaymentSettings> _companyPaymentSettingsProjection;

        public PaymentSettingGenerator(IProjectionSet<ServerPaymentSettings, string> networkCompanyPaymentSettingsProjections, IProjection<ServerPaymentSettings> companyPaymentSettingsProjection)
        {
            _networkCompanyPaymentSettingsProjections = networkCompanyPaymentSettingsProjections;
            _companyPaymentSettingsProjection = companyPaymentSettingsProjection;
        }

        public void Handle(PaymentSettingUpdated @event)
        {
            // migration for old events, set the default value
            if (!@event.ServerPaymentSettings.PreAuthAmount.HasValue)
            {
                @event.ServerPaymentSettings.PreAuthAmount = new ServerPaymentSettings().PreAuthAmount;
            }

            if (@event.ServerPaymentSettings.CompanyKey.HasValue())
            {
                _networkCompanyPaymentSettingsProjections.AddOrReplace(@event.ServerPaymentSettings);
            }
            else
            {
                _companyPaymentSettingsProjection.Save(@event.ServerPaymentSettings);
            }
        }
    }
}