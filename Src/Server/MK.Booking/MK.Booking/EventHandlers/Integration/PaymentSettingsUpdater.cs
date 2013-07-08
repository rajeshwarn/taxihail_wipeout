using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PaymentSettingsUpdater : 
        IEventHandler<PaymentModeChanged>
    {
        private IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;

        public PaymentSettingsUpdater(IAccountDao accountDao, ICommandBus commandBus)
        {
            _accountDao = accountDao;
            _commandBus = commandBus;
        }

        public void Handle(PaymentModeChanged @event)
        {
            _commandBus.Send(new DeleteAllCreditCards()
                {
                    AccountIds = _accountDao.GetAll().Select(a=>a.Id).ToArray()
                });   
         
        }
    }
}
