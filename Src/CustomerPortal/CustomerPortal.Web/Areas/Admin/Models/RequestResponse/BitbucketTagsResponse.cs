namespace CustomerPortal.Web.Areas.Admin.Repository
{
    public class BitbucketTagsResponse
    {
        public string node { get; set; }
        public object files { get; set; }
        public object raw_author { get; set; }
        public object utctimestamp { get; set; }
        public object author { get; set; }
        public object timestamp { get; set; }
        public object raw_node { get; set; }
        public object parents { get; set; }
        public object branch { get; set; }
        public object message { get; set; }
        public object revision { get; set; }
        public object size { get; set; }
    }
}