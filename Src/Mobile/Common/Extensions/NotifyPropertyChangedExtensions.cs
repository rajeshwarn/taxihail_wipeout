using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class NotifyPropertyChangedExtensions
    {
        public static IObservable<string> OnPropertyChanged(this INotifyPropertyChanged obj)
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                ev => obj.PropertyChanged += ev,
                ev => obj.PropertyChanged -= ev)
                .Select(e=>e.EventArgs.PropertyName);        
        }
    }
}

