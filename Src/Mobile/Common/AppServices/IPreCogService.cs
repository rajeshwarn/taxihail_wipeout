using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Cmt;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPreCogService
    {
        void Start();
        void SetUserLocation(Position position);
        PreCogResponse SendRequest(PreCogRequest request);
    }
}