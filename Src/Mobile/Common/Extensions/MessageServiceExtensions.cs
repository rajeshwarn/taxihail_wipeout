using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class MessageServiceExtensions
    {
        public static async Task<bool> ShowConfirmMessage(this IMessageService service,string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            var localization = await LocalizationProvider();
            
            service.ShowMessage(
                title, 
                message, 
                localization["OkButtonText"],
                () => tcs.TrySetResult(true), 
                localization["Cancel"], 
                () => tcs.TrySetResult(false)).FireAndForget();
            
            return await tcs.Task;
        }

        private static async Task<ILocalization> LocalizationProvider()
        {
            if (Mvx.CanResolve<ILocalization>())
            {
                return Mvx.Resolve<ILocalization>();
            }

            var tcs = new TaskCompletionSource<ILocalization>();

            Mvx.CallbackWhenRegistered<ILocalization>(service => tcs.TrySetResult(service));

            return await tcs.Task;
        }
    }
}