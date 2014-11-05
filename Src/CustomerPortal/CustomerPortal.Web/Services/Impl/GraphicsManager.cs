#region

using System.IO;
using System.Web.Hosting;

#endregion

namespace CustomerPortal.Web.Services.Impl
{
    public class GraphicsManager : FileManagerBase, IFileManager
    {
        private readonly string _companyId;

        public GraphicsManager(string companyId)
        {
            _companyId = companyId;
        }

        public override string GetFolderPath()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/uploads/" + _companyId + "/graphics");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}