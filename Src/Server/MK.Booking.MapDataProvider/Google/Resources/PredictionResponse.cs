#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Google.Resources	
{
    public class PredictionResponse : GoogleResult
    {
        public List<Prediction> predictions { get; set; }
    }
}