using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class PromotionCommandHandler : 
        ICommandHandler<CreatePromotion>,
        ICommandHandler<UpdatePromotion>,
        ICommandHandler<ActivatePromotion>,
        ICommandHandler<DeactivatePromotion>,
        ICommandHandler<UsePromotion>
    {
        private readonly IEventSourcedRepository<Promotion> _repository;

        public PromotionCommandHandler(IEventSourcedRepository<Promotion> repository)
        {
            _repository = repository;
        }

        public void Handle(CreatePromotion command)
        {
            var promotion = new Promotion(command.PromoId, command.Name, command.StartDate, command.EndDate, command.StartTime, 
                command.EndTime, command.DaysOfWeek, command.AppliesToCurrentBooking, command.AppliesToFutureBooking, 
                command.DiscountValue, command.DiscountType, command.MaxUsagePerUser, command.MaxUsage, command.Code);

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(UpdatePromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Update(command.Name, command.StartDate, command.EndDate, command.StartTime, command.EndTime, 
                command.DaysOfWeek, command.AppliesToCurrentBooking, command.AppliesToFutureBooking, command.DiscountValue, 
                command.DiscountType, command.MaxUsagePerUser, command.MaxUsage, command.Code);

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(ActivatePromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Activate();

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(DeactivatePromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Deactivate();

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(UsePromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Use(command.OrderId, command.AccountId, command.PickupDate, command.IsFutureBooking);

            _repository.Save(promotion, command.Id.ToString());
        }
    }
}