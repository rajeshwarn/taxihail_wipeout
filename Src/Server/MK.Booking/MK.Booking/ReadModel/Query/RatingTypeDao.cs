#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class RatingTypeDao : IRatingTypeDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public RatingTypeDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<RatingTypeDetail[]> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                var resultList = new List<RatingTypeDetail[]>();
                var allDetails = context.Query<RatingTypeDetail>().ToList();
                var uniqueIds = allDetails.Select(d => d.Id).Distinct();

                foreach (var ids in uniqueIds)
                {
                    resultList.Add(allDetails.Where(d => d.Id == ids).ToArray());
                }

                return resultList;
            }
        }

        public IList<RatingTypeDetail> GetById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<RatingTypeDetail>().Where(r => r.Id == id).ToList();
            }
        }

        public RatingTypeDetail FindByName(string name, string language)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<RatingTypeDetail>().SingleOrDefault(r => r.Name == name && r.Language == language);
            }
        }
    }
}