#region

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using CustomerPortal.Web.Distribution;
using CustomerPortal.Web.Domain;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Services;
using CustomerPortal.Web.Services.Impl;

#endregion

namespace CustomerPortal.Web.Controllers
{
    [AllowAnonymous]
    public class DistributionController : Controller
    {
        private readonly Func<string, string, IFileManager> _packageManagerFactory;

        public DistributionController(Func<string, string, IFileManager> packageManagerFactory)
        {
            _packageManagerFactory = packageManagerFactory;
        }

        public DistributionController()
            : this((id, version) => new PackageManager(id, version))
        {
        }

        public ActionResult Index(string id, string versionId)
        {
            var company = new CompanyService(id).GetCompany();
            var version = company.FindVersionById(versionId);
            if (version == null)
            {
                return HttpNotFound();
            }
            var viewModel = VersionViewModel.CreateFrom(company, version);
            return View(viewModel);
        }

        [RequireHttps]
        public ActionResult Manifest(string id, string number, string package)
        {
            string filepath;
            var packages = new PackageManager(id, number);
            if (!packages.Exists(package, out filepath))
            {
                return HttpNotFound();
            }

            var plist = new InfoPlistExtractor().ExtractFromPackage(filepath);
            var viewModel = new ManifestViewModel
            {
                BundleIdentifier = GetPropertyValue(plist, "CFBundleIdentifier"),
                BundleVersion = GetPropertyValue(plist, "CFBundleVersion"),
                Title = GetPropertyValue(plist, "CFBundleDisplayName"),
                DisplayImage =
                    GetApplicationPath(Request) +
                    Url.Action("DisplayImage",
                        new {id, number, package, image = GetArrayValues(plist, "CFBundleIconFiles")[0]}),
                FullSizeImage = GetApplicationPath(Request) + Url.Content("~/Content/512x512.png"),
                SoftwarePackage = GetApplicationPath(Request) + Url.Action("Package", new {id, number, package}),
            };


            Response.ContentType = "text/xml";
            return View(viewModel);
        }

        public ActionResult DisplayImage(string id, string number, string package, string image)
        {
            string filepath;
            var packages = new PackageManager(id, number);
            if (!packages.Exists(package, out filepath))
            {
                return HttpNotFound();
            }

            using (var archive = ZipFile.OpenRead(filepath))
            {
                var imageStream = archive.Entries
                    .Where(x => x.Name == image)
                    .OrderBy(x => x.FullName.Length)
                    .First().Open();
                var fileContents = new MemoryStream();
                imageStream.CopyTo(fileContents);
                fileContents.Seek(0, SeekOrigin.Begin);
                imageStream.Dispose();
                return File(fileContents, "image/png");
            }
        }

        public ActionResult Package(string id, string number, string package)
        {
            var packages = _packageManagerFactory.Invoke(id, number);
            string filepath;
            if (!packages.Exists(package, out filepath))
            {
                return HttpNotFound();
            }
            var mimeType = MimeMapping.GetMimeMapping(filepath);
            return base.File(filepath, mimeType);
        }

        [NonAction]
        private string GetPropertyValue(XDocument plist, string propertyName)
        {
            return plist.Root.Descendants("key")
                .First(x => x.Value == propertyName)
                .ElementsAfterSelf("string").First().Value;
        }

        private string[] GetArrayValues(XDocument plist, string propertyName)
        {
            return plist.Root.Descendants("key")
                .First(x => x.Value == propertyName)
                .ElementsAfterSelf("array").First()
                .Elements().Select(x => x.Value).ToArray();
        }

        public static string GetApplicationPath(HttpRequestBase httpRequest)
        {
            var root = httpRequest.Url.GetLeftPart(UriPartial.Authority);
            return root;
        }
    }
}