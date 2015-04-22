#region

using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using RestSharp.Extensions;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PaymentSettingsUpdater :
        IIntegrationEventHandler,
        IEventHandler<PaymentModeChanged>,
        IEventHandler<PayPalSettingsChanged>,
        IEventHandler<ChargeAccountChanged>
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

        public void Handle(PayPalSettingsChanged @event)
        {
            _commandBus.Send(new UnlinkAllPayPalAccounts
            {
                AccountIds = _accountDao.GetAll().Select(a => a.Id).ToArray()
            });
        }

        public void Handle(ChargeAccountChanged @event)
        {
            var accountIds = _accountDao.GetAll()
                .Where(a => a.Settings.CustomerNumber.HasValue() || a.Settings.AccountNumber.HasValue())
                .Select(a => a.Id)
                .ToArray();

            _commandBus.Send(new ClearChargeAccountUserSettings
            {
                AccountIds = accountIds
            });
        }
    }
}