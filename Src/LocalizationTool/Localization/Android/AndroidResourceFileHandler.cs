using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.Tools.Localization.Android
{
    public class AndroidResourceFileHandler : IResourceFileHandler
    {

        private string _fileName;
        public void Load(string fileName)
        {
            _fileName = fileName;
        }
        public string Name
        {
            get { return Path.GetFileName(_fileName); }
        }

        public string[] Keys
        {
            get { throw new NotImplementedException(); }
        }

        public string GetValue(string key)
        {
            throw new NotImplementedException();
        }

        public void SetValue(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
