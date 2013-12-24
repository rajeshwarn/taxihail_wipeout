// ReSharper disable once CheckNamespace
namespace CustomerPortal.Web.Entities
{
    public enum EnvironmentRole
    {
        BuildServer = 1,
        BuildMobile = 2,
        DeployServer = 3,
    }

    public class Environment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string SqlServerInstance { get; set; }
        public string WebSitesFolder { get; set; }
        public EnvironmentRole Role { get; set; }
        public string DeployFolder { get; set; }
        public string DropFolder { get; set; }
    }
}