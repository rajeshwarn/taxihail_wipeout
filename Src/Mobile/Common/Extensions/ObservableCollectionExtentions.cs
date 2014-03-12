using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class ObservableCollectionExtentions
    {
        public static void AddMultiple<T>(this ObservableCollection<T> thisCollection, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                thisCollection.Add(item);
            }
        }
    }
}

