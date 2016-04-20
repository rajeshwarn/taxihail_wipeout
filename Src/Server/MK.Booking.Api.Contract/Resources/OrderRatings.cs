using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Serializer;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderRatings : BaseDto
    {
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid OrderId { get; set; }
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
    }
}