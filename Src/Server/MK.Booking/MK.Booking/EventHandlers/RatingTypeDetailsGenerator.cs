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
    public class RatingTypeDetailsGenerator :
        IEventHandler<RatingTypeAdded>,
        IEventHandler<RatingTypeHidded>,
        IEventHandler<RatingTypeUpdated>,
        IEventHandler<RatingTypeDeleted>
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
                        Name = @event.Name,
                        Language = @event.Language
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
                var ratingType = context.Set<RatingTypeDetail>().Find(@event.RatingTypeId, @event.Language);
                ratingType.Name = @event.Name;
                ratingType.Language = @event.Language;
                context.SaveChanges();
            }
        }

        public void Handle(RatingTypeDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                foreach (var language in @event.Languages)
                {
                    var ratingType = context.Set<RatingTypeDetail>().Find(@event.RatingTypeId, language);
                    if (ratingType != null)
                    {
                        context.Set<RatingTypeDetail>().Remove(ratingType);
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}