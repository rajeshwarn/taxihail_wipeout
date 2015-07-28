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
        ICommandHandler<ApplyPromotion>,
        ICommandHandler<UnapplyPromotion>,
        ICommandHandler<RedeemPromotion>,
        ICommandHandler<AddUserToPromotionWhiteList>,
        ICommandHandler<DeletePromotion>
    {
        private readonly IEventSourcedRepository<Promotion> _repository;

        public PromotionCommandHandler(IEventSourcedRepository<Promotion> repository)
        {
            _repository = repository;
        }

        public void Handle(CreatePromotion command)
        {
            var promotion = new Promotion(command.PromoId, command.Name, command.Description, command.StartDate, command.EndDate, 
                command.StartTime, command.EndTime, command.DaysOfWeek, command.AppliesToCurrentBooking, command.AppliesToFutureBooking,
                command.DiscountValue, command.DiscountType, command.MaxUsagePerUser, command.MaxUsage, command.Code, 
                command.PublishedStartDate, command.PublishedEndDate, command.TriggerSettings);

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(UpdatePromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Update(command.Name, command.Description, command.StartDate, command.EndDate, command.StartTime, command.EndTime, 
                command.DaysOfWeek, command.AppliesToCurrentBooking, command.AppliesToFutureBooking, command.DiscountValue, command.DiscountType, 
                command.MaxUsagePerUser, command.MaxUsage, command.Code, command.PublishedStartDate, command.PublishedEndDate, command.TriggerSettings);

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

        public void Handle(DeletePromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Delete();

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(ApplyPromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Apply(command.OrderId, command.AccountId, command.PickupDate, command.IsFutureBooking);

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(UnapplyPromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Unapply(command.OrderId, command.AccountId);

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(RedeemPromotion command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.Redeem(command.OrderId, command.TaxedMeterAmount, command.TipAmount);

            _repository.Save(promotion, command.Id.ToString());
        }

        public void Handle(AddUserToPromotionWhiteList command)
        {
            var promotion = _repository.Get(command.PromoId);

            promotion.AddUserToWhiteList(command.AccountIds, command.LastTriggeredAmount);

            _repository.Save(promotion, command.Id.ToString());
        }
    }
}