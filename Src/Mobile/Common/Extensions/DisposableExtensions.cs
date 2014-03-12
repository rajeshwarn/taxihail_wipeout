using System;
using System.Reactive.Disposables;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class DisposableExtensions
    {
		public static void DisposeWith(this IDisposable disposable, CompositeDisposable composite)
        {
            if(disposable == null) throw new ArgumentNullException("disposable");
            if(composite == null) throw new ArgumentNullException("composite");

            composite.Add (disposable);
        }

        public static void DisposeAll (this CompositeDisposable composite)
        {
            if (composite == null)
                throw new ArgumentNullException ("composite");

            foreach (var disposable in composite) {
                disposable.Dispose();
            }
            composite.Clear();

        }
    }
}

