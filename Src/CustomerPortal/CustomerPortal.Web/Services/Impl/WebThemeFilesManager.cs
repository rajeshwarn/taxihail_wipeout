using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using CustomerPortal.Web.Helpers;

namespace CustomerPortal.Web.Services.Impl
{
    public class WebThemeFilesManager : FileManagerBase
    {
        private readonly string _companyId;

        public WebThemeFilesManager(string companyId)
        {
            _companyId = companyId;
        }

        public override string GetFolderPath()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/uploads/" + _companyId + "/webtheme");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public void CopyTo(string destinationCompanyId, params string[] excludeFiles)
        {
            var m = new AssetsManager(destinationCompanyId);
            var dest = m.GetFolderPath();

            foreach (var file in GetAll())
            {
                var destFile = Path.Combine(m.GetFolderPath(), Path.GetFileName(file));
                if (!File.Exists(destFile) && !excludeFiles.Any(x => x.ToLower() == Path.GetFileName(file).ToLower()))
                {
                    File.Copy(file, destFile);
                }
            }



        }

        public Color? GetAccentColor()
        {
            var variable = GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "variables.less");
            if (variable == null) 
            {
                return null;
            }
            var hexValue = Regex.Match(System.IO.File.ReadAllText(variable), @"@taxiHailAccentColor:(.+?);").Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(hexValue) && hexValue.StartsWith("#"))
            {
                return ColorHelper.ColorFromHex("#ff"+hexValue.Substring(1, hexValue.Length - 1));
            }
            return null;
        }

        public void SetAccentColor(Color color)
        {
            var variable = GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "variables.less");

            if (variable == null)
            { return; }

            var variableContent = System.IO.File.ReadAllText(variable);
            variableContent = Regex.Replace(variableContent, @"@taxiHailAccentColor:(.+?);",
                string.Format(@"@taxiHailAccentColor:#{0};", ColorHelper.HexFromColor(color)));
            System.IO.File.WriteAllText(variable, variableContent, Encoding.UTF8);
           
        }

        public Color? GetEmailFontColor()
        {
            var variable = GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "variables.less");
            if (variable == null)
            {
                return null;
            }
            var hexValue = Regex.Match(System.IO.File.ReadAllText(variable), @"@taxiHailEmailFontColor:(.+?);").Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(hexValue) && hexValue.StartsWith("#"))
            {
                return ColorHelper.ColorFromHex("#00" + hexValue.Substring(1, hexValue.Length - 1));
            }
            return null;
        }

        public void SetEmailFontColor(Color color)
        {
            var variable = GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "variables.less");

            if (variable == null)
            { return; }

            var variableContent = System.IO.File.ReadAllText(variable);
            variableContent = Regex.Replace(variableContent, @"@taxiHailEmailFontColor:(.+?);",
                string.Format(@"@taxiHailEmailFontColor:#{0};", ColorHelper.HexFromColor(color)));
            System.IO.File.WriteAllText(variable, variableContent, Encoding.UTF8);

        }
    }
}