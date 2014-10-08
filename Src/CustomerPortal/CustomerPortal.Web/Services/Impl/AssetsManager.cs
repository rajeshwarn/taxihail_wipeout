#region

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;

#endregion

namespace CustomerPortal.Web.Services.Impl
{
    public class AssetsManager : FileManagerBase
    {
        private readonly string _companyId;

        public AssetsManager(string companyId)
        {
            _companyId = companyId;
        }

        public override string GetFolderPath()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/uploads/" + _companyId + "/assets");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public void CopyTo( string destinationCompanyId, params string[] excludeFiles )
        {
            var m = new AssetsManager( destinationCompanyId );
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

        public Color? GetStyleNavigationBarColor()
        {
            var style = GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "style.json");
            if (style == null)
            {
                return null;
            }

            dynamic jsonStyle = JObject.Parse(System.IO.File.ReadAllText(style));

            if (jsonStyle.NavigationTitleColor == null)
            {
                return null;
            }


            return Color.FromArgb(Convert.ToInt32(jsonStyle.NavigationBarColor.Red.ToString()),
                                    Convert.ToInt32(jsonStyle.NavigationBarColor.Green.ToString()),
                                    Convert.ToInt32(jsonStyle.NavigationBarColor.Blue.ToString()));

        }

        public Color? GetStyleNavigationTitleBarColor()
        {
            var style = GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "style.json");
            if (style == null)
            {
                return null;
            }

            dynamic jsonStyle = JObject.Parse(System.IO.File.ReadAllText(style));

            if (jsonStyle.NavigationTitleColor == null)
            {
                return null;
            }

            return Color.FromArgb(Convert.ToInt32(jsonStyle.NavigationTitleColor.Red.ToString()),
                                    Convert.ToInt32(jsonStyle.NavigationTitleColor.Green.ToString()),
                                    Convert.ToInt32(jsonStyle.NavigationTitleColor.Blue.ToString()));

        }

        public void SetStyleNavigationBarColor(Color color)
        {
            var style = GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "style.json");
            if (style == null)
            { return; }

            dynamic jsonStyle = JObject.Parse(System.IO.File.ReadAllText(style));

            jsonStyle.NavigationBarColor.Red = color.R.ToString();
            jsonStyle.NavigationBarColor.Green = color.G.ToString();
            jsonStyle.NavigationBarColor.Blue = color.B.ToString();
            var content = jsonStyle.ToString();
             
            System.IO.File.WriteAllText(style, content, Encoding.UTF8);

        }

        public void SetStyleNavigationTitleBarColor(Color color)
        {
            
            var style = GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "style.json");
            if ( style == null )
            { return; }

            dynamic jsonStyle = JObject.Parse(System.IO.File.ReadAllText(style));
            jsonStyle.NavigationTitleColor.Red = color.R.ToString();
            jsonStyle.NavigationTitleColor.Green = color.G.ToString();
            jsonStyle.NavigationTitleColor.Blue = color.B.ToString();
            var content = jsonStyle.ToString();             
            System.IO.File.WriteAllText(style, content, Encoding.UTF8);
        }
    }
}