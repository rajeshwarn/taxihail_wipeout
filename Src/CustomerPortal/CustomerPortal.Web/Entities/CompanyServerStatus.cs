
using CustomerPortal.Contract.Resources;
using MongoRepository;

namespace CustomerPortal.Web.Entities
{
    public class CompanyServerStatus : IEntity
    {
        public string CompanyName { get; set; }
        public string CompanyKey { get; set; }
        public bool IsProduction { get; set; }
        public ServiceStatus ServiceStatus { get; set; }

        public bool HasNoStatusApi { get; set; }
        public string Id { get; set; }
    }
}
