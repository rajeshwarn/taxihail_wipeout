using System;

namespace apcurium.MK.Common.Entity
{
    public class RatingTypeWrapper
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public RatingType[] RatingTypes { get; set; }
    }
}