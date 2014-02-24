using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

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

