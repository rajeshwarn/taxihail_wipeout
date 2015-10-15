#region

using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
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

        public PaymentSettingsUpdater(IAccountDao accountDao, ICommandBus commandBus)
        {
            _accountDao = accountDao;
            _commandBus = commandBus;
        }

        public void Handle(PaymentModeChanged @event)
        {
            _commandBus.Send(new DeleteCreditCardsFromAccounts
            {
                AccountIds = _accountDao.GetAll().Select(a => a.Id).ToArray()
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