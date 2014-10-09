#region

using System.IO;
using System.Web.Hosting;

#endregion

namespace CustomerPortal.Web.Services.Impl
{
    public class LayoutsManager : FileManagerBase, IFileManager
    {
        private readonly string _companyId;

        public LayoutsManager(string companyId)
        {
            _companyId = companyId;
        }

        public override string GetFolderPath()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/uploads/" + _companyId + "/layout");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}