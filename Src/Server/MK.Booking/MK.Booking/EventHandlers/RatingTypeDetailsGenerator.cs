using System;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class RatingTypeDetailsGenerator :
        IEventHandler<RatingTypeAdded>,
        IEventHandler<RatingTypeHidded>,
        IEventHandler<RatingTypeUpdated>,
        IEventHandler<RatingTypeDeleted>
    {
        private readonly RatingTypeDetailProjectionSet _ratingTypeProjectionSet;

        public RatingTypeDetailsGenerator(RatingTypeDetailProjectionSet ratingTypeProjectionSet)
        {
            _ratingTypeProjectionSet = ratingTypeProjectionSet;
        }

        public void Handle(RatingTypeAdded @event)
        {
            // Support for old events without language property
            var languages = @event.Language.IsNullOrEmpty() ? Enum.GetNames(typeof(SupportedLanguages)) : new[] { @event.Language };

            foreach (var language in languages)
            {
                _ratingTypeProjectionSet.Update(@event.RatingTypeId, list => 
                {
                    var existing = list.FirstOrDefault(x => x.Id == @event.RatingTypeId
                                                         && x.Name == @event.Name
                                                         && x.Language == language);

                    if (existing == null)
                    {
                        list.Add(new RatingTypeDetail
                        {
                            CompanyId = @event.SourceId,
                            Id = @event.RatingTypeId,
                            Name = @event.Name,
                            Language = language
                        });
                    }
                });
            }
        }

        public void Handle(RatingTypeHidded @event)
        {
            _ratingTypeProjectionSet.Update(@event.RatingTypeId, list =>
            {
                foreach (var ratingType in list)
                {
                    ratingType.IsHidden = true;
                }
            });
        }

        public void Handle(RatingTypeUpdated @event)
        {
            // Support for old events without language property
            var languages = @event.Language.IsNullOrEmpty() ? Enum.GetNames(typeof(SupportedLanguages)) : new[] { @event.Language };

            foreach (var language in languages)
            {
                _ratingTypeProjectionSet.Update(@event.RatingTypeId, list =>
                {
                    var ratingType = list.FirstOrDefault(x => x.Id == @event.RatingTypeId && x.Language == language);
                    if (ratingType != null)
                    {
                        ratingType.Name = @event.Name;
                        ratingType.Language = @event.Language;
                    }
                });
            }
        }

        public void Handle(RatingTypeDeleted @event)
        {
            _ratingTypeProjectionSet.Remove(@event.RatingTypeId);
        }
    }
}