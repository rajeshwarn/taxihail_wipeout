using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Services
{
    public interface IServiceTypeService
    {
        string GetIBSWebServicesUrl(ServiceType serviceType);
    }
}