#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
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
                var languages = @event.Language.IsNullOrEmpty() ? Enum.GetNames(typeof(SupportedLanguages)) : new []{ @event.Language };

                foreach (var language in languages)
                {
                    var duplicate = context.Query<RatingTypeDetail>()
                                           .FirstOrDefault(x => x.Id == @event.RatingTypeId
                                                                && x.Name == @event.Name
                                                                && x.Language == language);
                    if (duplicate == null)
                    {
                        context.Save(new RatingTypeDetail
                        {
                            CompanyId = @event.SourceId,
                            Id = @event.RatingTypeId,
                            Name = @event.Name,
                            Language = language
                        });
                    }
                }
            }
        }

        public void Handle(RatingTypeHidded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var ratingTypes = context.Set<RatingTypeDetail>().Where(x => x.Id == @event.RatingTypeId);
                foreach (var ratingType in ratingTypes)
                {
                    ratingType.IsHidden = true; 
                }
                context.SaveChanges();
            }
        }

        public void Handle(RatingTypeUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                // Support for old events without language property
                var languages = @event.Language.IsNullOrEmpty() ? Enum.GetNames(typeof(SupportedLanguages)) : new[] { @event.Language };
                foreach (var language in languages)
                {
                    var ratingType = context.Set<RatingTypeDetail>().Find(@event.RatingTypeId, language);
                    ratingType.Name = @event.Name;
                    ratingType.Language = @event.Language;
                }
                context.SaveChanges();
            }
        }

        public void Handle(RatingTypeDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var ratingTypes = context.Query<RatingTypeDetail>().Where(x => x.Id == @event.RatingTypeId);
                foreach (var ratingType in ratingTypes)
                {
                    context.Set<RatingTypeDetail>().Remove(ratingType);
                }
                context.SaveChanges();
            }
        }
    }
}