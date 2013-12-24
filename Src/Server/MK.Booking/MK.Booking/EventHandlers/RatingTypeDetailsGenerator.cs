#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class RatingTypeDetailsGenerator : IEventHandler<RatingTypeAdded>, IEventHandler<RatingTypeHidded>,
        IEventHandler<RatingTypeUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public RatingTypeDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(RatingTypeAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                if (context.Query<RatingTypeDetail>().FirstOrDefault(x => x.Name == @event.Name) == null)
                {
                    context.Save(new RatingTypeDetail
                    {
                        CompanyId = @event.SourceId,
                        Id = @event.RatingTypeId,
                        Name = @event.Name
                    });
                }
            }
        }

        public void Handle(RatingTypeHidded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var ratingType = context.Find<RatingTypeDetail>(@event.RatingTypeId);
                ratingType.IsHidden = true;

                context.SaveChanges();
            }
        }

        public void Handle(RatingTypeUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var ratingType = context.Find<RatingTypeDetail>(@event.RatingTypeId);
                ratingType.Name = @event.Name;

                context.SaveChanges();
            }
        }
    }
}