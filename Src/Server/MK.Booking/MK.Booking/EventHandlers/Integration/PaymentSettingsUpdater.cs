using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PaymentSettingsUpdater :
        IIntegrationEventHandler,
        IEventHandler<PaymentModeChanged>
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
            _commandBus.Send(new DeleteAllCreditCards
            {
                AccountIds = _accountDao.GetAll().Select(a => a.Id).ToArray()
            });
        }
    }
}