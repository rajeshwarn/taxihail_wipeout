#region

using MongoRepository;

#endregion

namespace CustomerPortal.Web.Entities
{
    public class IosDevice : IEntity
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}