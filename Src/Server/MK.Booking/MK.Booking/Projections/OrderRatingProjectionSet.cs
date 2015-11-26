using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using EntityFramework.Utilities;

namespace apcurium.MK.Booking.Projections
{
    public abstract class OrderRatingProjectionSet : IAppendOnlyProjectionSet<Tuple<OrderRatingDetails, RatingScoreDetails[]>>
    {
        public abstract void Add(Tuple<OrderRatingDetails, RatingScoreDetails[]> projection);
        public abstract void AddRange(IEnumerable<Tuple<OrderRatingDetails, RatingScoreDetails[]>> projections);
    }

    public class OrderRatingMemoryProjectionSet : OrderRatingProjectionSet, IEnumerable<Tuple<OrderRatingDetails, RatingScoreDetails[]>>
    {
        readonly IDictionary<Guid, Tuple<OrderRatingDetails, RatingScoreDetails[]>> _cache = new Dictionary<Guid, Tuple<OrderRatingDetails, RatingScoreDetails[]>>();
        public override void Add(Tuple<OrderRatingDetails, RatingScoreDetails[]> projection)
        {
            _cache.Add(projection.Item1.OrderId, projection);
        }

        public override void AddRange(IEnumerable<Tuple<OrderRatingDetails, RatingScoreDetails[]>> projections)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Tuple<OrderRatingDetails, RatingScoreDetails[]>> GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }
    }

    public class OrderRatingEntityProjectionSet : OrderRatingProjectionSet
    {
        readonly Func<BookingDbContext> _contextFactory;

        public OrderRatingEntityProjectionSet(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override void Add(Tuple<OrderRatingDetails, RatingScoreDetails[]> projection)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Set<OrderRatingDetails>().Add(projection.Item1);
                context.Set<RatingScoreDetails>().AddRange(projection.Item2);
                context.SaveChanges();
            }
        }

        public override void AddRange(IEnumerable<Tuple<OrderRatingDetails, RatingScoreDetails[]>> projections)
        {
            using (var context = _contextFactory.Invoke())
            {
                EFBatchOperation.For(context, context.Set<OrderRatingDetails>()).InsertAll(projections.Select(x => x.Item1));
                EFBatchOperation.For(context, context.Set<RatingScoreDetails>()).InsertAll(projections.SelectMany(x => x.Item2));
            }
        }
    }
}