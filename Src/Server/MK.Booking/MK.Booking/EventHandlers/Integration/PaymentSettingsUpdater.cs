#region

using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PaymentSettingsUpdater :
        IIntegrationEventHandler,
        IEventHandler<PaymentModeChanged>,
        IEventHandler<PayPalSettingsChanged>
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;

        public PaymentSettingsUpdater(IAccountDao accountDao, ICommandBus commandBus, IServerSettings serverSettings)
        {
            _accountDao = accountDao;
            _commandBus = commandBus;
            _serverSettings = serverSettings;
        }

        public void Handle(PaymentModeChanged @event)
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            var forceUserDisconnect = paymentSettings.CreditCardIsMandatory
                    && paymentSettings.IsPaymentOutOfAppDisabled != OutOfAppPaymentDisabled.None;

            _commandBus.Send(new DeleteCreditCardsFromAccounts
            {
                AccountIds = _accountDao.GetAll().Select(a => a.Id).ToArray(),
                ForceUserDisconnect = forceUserDisconnect
            });
        }

        public void Handle(PayPalSettingsChanged @event)
        {
            _commandBus.Send(new UnlinkAllPayPalAccounts
            {
                AccountIds = _accountDao.GetAll().Select(a => a.Id).ToArray()
            });
        }
    }
}