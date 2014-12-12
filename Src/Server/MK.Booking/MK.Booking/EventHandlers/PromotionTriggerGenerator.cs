using System;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PromotionTriggerGenerator :
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<AccountConfirmed>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        private readonly IPromotionDao _promotionDao;

        public PromotionTriggerGenerator(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            IPromotionDao promotionDao)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _promotionDao = promotionDao;
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            throw new NotImplementedException();
        }

        public void Handle(AccountConfirmed @event)
        {
            // Check if exists promotion triggered on account creation
            var activePromotions = _promotionDao.GetAllCurrentlyActive();
            var accountCreatedPromotion =
                activePromotions.FirstOrDefault(
                    p => p.TriggerSettings.Type == PromotionTriggerTypes.AccountCreated.Id);

            if (accountCreatedPromotion != null)
            {
                _commandBus.Send(new AddUserToPromotionWhiteList
                {
                    AccountId = @event.SourceId,
                    PromoId = accountCreatedPromotion.Id
                });
            }
        }
    }
}
