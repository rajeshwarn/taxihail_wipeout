#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using MK.Common.Android.Serializer;
using Newtonsoft.Json;

#endregion

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