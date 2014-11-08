using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class PromotionCommandHandler : ICommandHandler<CreatePromotion>
    {
        private readonly IEventSourcedRepository<Promotion> _repository;

        public PromotionCommandHandler(IEventSourcedRepository<Promotion> repository)
        {
            _repository = repository;
        }

        public void Handle(CreatePromotion command)
        {
            var promotion = new Promotion(command.PromoId);

            _repository.Save(promotion, command.Id.ToString());
        }
    }
}