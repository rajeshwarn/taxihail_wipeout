#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.Google.Resources
{
    public class Prediction
    {
        public string Description { get; set; }
        public string Id { get; set; }
        public string Reference { get; set; }
        public List<string> Types { get; set; }
    }
}