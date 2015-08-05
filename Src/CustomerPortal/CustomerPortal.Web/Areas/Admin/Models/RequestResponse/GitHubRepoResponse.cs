namespace CustomerPortal.Web.Areas.Admin.Repository
{
    public class GitHubRepoResponse
    {
       public string Name { get; set; }
        public Commit Commit { get; set; }
    }

    public class Commit
    {
        public string Sha { get; set; }
        public string Url { get; set; }
    }

}