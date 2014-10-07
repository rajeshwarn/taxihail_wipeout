#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

#endregion

namespace CustomerPortal.Web.Services.Impl
{
    public abstract class FileManagerBase : IFileManager
    {
        public bool Delete(string file)
        {
            var filepath = Path.Combine(GetFolderPath(), Path.GetFileName(file));
            if (!File.Exists(filepath))
            {
                return false;
            }
            File.Delete(filepath);
            return true;
        }

        public void DeleteAll()
        {
            Directory.Delete(GetFolderPath(), true);
        }

        public void Save(HttpPostedFileBase file)
        {
            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(GetFolderPath(), fileName);
            file.SaveAs(path);
        }

        public IEnumerable<string> GetAll()
        {
            return Directory.EnumerateFiles(GetFolderPath()).ToArray();
        }

        public bool Exists(string file, out string filepath)
        {
            filepath = Path.Combine(GetFolderPath(), Path.GetFileName(file));
            return File.Exists(filepath);
        }

        public void Save(string fileName, Stream file)
        {
            var path = Path.Combine(GetFolderPath(), fileName);
            using (var fileStream = File.Create(path, (int) file.Length))
            {
                var bytesInStream = new byte[file.Length];
                file.Read(bytesInStream, 0, bytesInStream.Length);
                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                fileStream.Close();
            }
        }

        public abstract string GetFolderPath();
    }
}