#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Resources
{
    public class PredictionResponse
    {
        public List<Prediction> predictions { get; set; }
        public string status { get; set; }
    }
}