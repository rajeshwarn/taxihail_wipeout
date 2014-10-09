#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CustomerPortal.Web.Services;

#endregion

namespace CustomerPortal.Web.Test.Helpers
{
    public class FakeFileManager : IFileManager
    {
        private readonly string _directoryPath;
        private readonly List<string> _files = new List<string>();

        public FakeFileManager(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        public bool Delete(string file)
        {
            return _files.Remove(file);
        }

        public void DeleteAll()
        {
            _files.Clear();
        }

        public void Save(HttpPostedFileBase file)
        {
            _files.Add(Path.GetFileName(file.FileName));
        }

        public IEnumerable<string> GetAll()
        {
            return _files.AsEnumerable();
        }

        public bool Exists(string file, out string filepath)
        {
            filepath = Path.Combine(_directoryPath, file);

            return _files.Contains(file);
        }
    }
}