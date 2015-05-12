using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PromotionService : BaseService, IPromotionService
    {
        public Task<ActivePromotion[]> GetActivePromotions()
        {
            return UseServiceClientAsync<CompanyServiceClient, ActivePromotion[]>(client => client.GetActivePromotions());
        }
    }
}