#region

using System.Web;

#endregion

namespace CustomerPortal.Web.Test.Helpers
{
    public class PostedFileStub : HttpPostedFileBase
    {
        private readonly string _filename;

        public PostedFileStub(string filename)
        {
            _filename = filename;
        }

        public override int ContentLength
        {
            get { return 1000; }
        }

        public override string FileName
        {
            get { return _filename; }
        }

        public override string ContentType
        {
            get { return "application/archive"; }
        }
    }
}