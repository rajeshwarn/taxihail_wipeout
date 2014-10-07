#region

using System.Collections.Generic;
using System.Web;

#endregion

namespace CustomerPortal.Web.Services
{
    public interface IFileManager
    {
        bool Delete(string file);
        void Save(HttpPostedFileBase file);
        IEnumerable<string> GetAll();
        bool Exists(string file, out string filepath);
        void DeleteAll();
    }
}