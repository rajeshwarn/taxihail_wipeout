using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Domain
{
    public class Promotion : EventSourced
    {
        public Promotion(Guid id) : base(id)
        {
            Handles<PromotionCreated>(NoAction);
        }

        public Promotion(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public Promotion(Guid id, string name)
            : this(id)
        {
            if (Params.Get(name).Any(p => StringExtensions.IsNullOrEmpty(p)))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new PromotionCreated
            {
                SourceId = id,
                Name = name
            });
        }
    }
}