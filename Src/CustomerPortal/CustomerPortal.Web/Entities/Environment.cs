#region

using MongoRepository;

#endregion

namespace CustomerPortal.Web.Entities
{
    public enum EnvironmentRole
    {
        BuildServer = 1,
        BuildMobile = 2,
        DeployServer = 3,
    }

    public class Environment : IEntity
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public EnvironmentRole Role { get; set; }
        public string SqlServerInstance { get; set; }
        public string WebSitesFolder { get; set; }
        public bool IsProduction { get; set; }
        public string Id { get; set; }
    }
}