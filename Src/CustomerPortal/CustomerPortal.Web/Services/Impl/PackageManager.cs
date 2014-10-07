#region

using System.IO;
using System.Web.Hosting;

#endregion

namespace CustomerPortal.Web.Services.Impl
{
    public class PackageManager : FileManagerBase
    {
        private readonly string _companyId;
        private readonly string _version;

        public PackageManager(string companyId, string version)
        {
            _companyId = Path.GetFileName(companyId);
            _version = Path.GetFileName(version);
        }

        public override string GetFolderPath()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/uploads/" + _companyId + "/versions/" + _version);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}