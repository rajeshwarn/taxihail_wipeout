#region

using System.IO;
using System.Web.Hosting;

#endregion

namespace CustomerPortal.Web.Services.Impl
{
    public class PackagesManager : FileManagerBase, IFileManager
    {
        public override string GetFolderPath()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/uploads/packages");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}