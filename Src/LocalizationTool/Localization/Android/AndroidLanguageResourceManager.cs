using System.IO;
using System.Linq;
using System.Xml;

namespace apcurium.Tools.Localization.Android
{
    public static class AndroidLanguageResourceManager
    {
        public static void CreateResourceFileIfNecessary(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return;
            }

            const string androidResourcesBasePath = @"..\Mobile\Android\Resources";
            const string androidTaxiHailProjectPath = @"..\Mobile\Android\TaxiHail.csproj";

            string folderAndFileName = string.Format(@"values-{0}\String.xml", language);
            string androidLanguageFileName = string.Format(@"{0}\{1}", androidResourcesBasePath, folderAndFileName);

            if (!File.Exists(androidLanguageFileName))
            {
                Directory.CreateDirectory(string.Format(@"{0}\values-{1}", androidResourcesBasePath, language));
                var stringFile = File.Create(androidLanguageFileName);
                stringFile.Close();

                // Create blank language resource file
                using (var streamWriter = new StreamWriter(androidLanguageFileName))
                {
                    streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    streamWriter.WriteLine("<resources>");
                    streamWriter.WriteLine("</resources>");
                    streamWriter.Close();
                }

                // Add resource file to VS project
                var resourceFile = string.Format(@"Resources\{0}", folderAndFileName);

                var csProDocument = new XmlDocument();
                csProDocument.Load(androidTaxiHailProjectPath);

                XmlNode root = csProDocument.DocumentElement;
                if (root != null)
                {
                    XmlElement node = csProDocument.CreateElement("AndroidResource", root.NamespaceURI);
                    node.SetAttribute("Include", resourceFile);

                    foreach (var child in root.ChildNodes)
                    {
                        var childNode = child as XmlNode;
                        if (childNode != null
                            && childNode.Name == "ItemGroup"
                            && childNode.ChildNodes.OfType<XmlNode>().Any(c => c.Name == "AndroidResource"))
                        {
                            // Append the new resource with the other AndroidResource tags
                            childNode.AppendChild(node);
                            break;
                        }
                    }
                }

                csProDocument.Save(androidTaxiHailProjectPath);
            }
        }
    }
}
