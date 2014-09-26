#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
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
                // Support for old events without language property
                var language = @event.Language ?? SupportedLanguages.en.ToString();

                if (context.Query<RatingTypeDetail>().FirstOrDefault(x => x.Name == @event.Name && x.Language == language) == null)
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
                // Support for old events without language property
                var language = @event.Language ?? SupportedLanguages.en.ToString();

                var ratingType = context.Set<RatingTypeDetail>().Find(@event.RatingTypeId, language);
                ratingType.Name = @event.Name;
                ratingType.Language = @event.Language;
                context.SaveChanges();
            }
        }

        public void Handle(RatingTypeDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                // Support for old events without language property
                var languages = @event.Languages ?? new [] { SupportedLanguages.en.ToString() };

                foreach (var language in languages)
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