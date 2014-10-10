using System.IO;

namespace apcurium.Tools.Localization.Android
{
    public static class AndroidLanguageFileManager
    {
        public static void CreateResourceFileIfNecessary(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                // English will always already be created
                return;
            }

            string resourcesBasePath = Path.GetFullPath(Path.Combine("..", "Mobile", "Android", "Resources"));
            string taxiHailProjectPath = Path.GetFullPath(Path.Combine("..", "Mobile", "Android", "TaxiHail.csproj"));

            string folderAndFileName = Path.Combine(string.Format("values-{0}", language), "String.xml");
            string languageFileName = Path.Combine(resourcesBasePath, folderAndFileName);

            if (!File.Exists(languageFileName))
            {
                Directory.CreateDirectory(Path.Combine(resourcesBasePath, string.Format("values-{0}", language)));
                var stringFile = File.Create(languageFileName);
                stringFile.Close();

                // Create blank language resource file
                using (var streamWriter = new StreamWriter(languageFileName))
                {
                    streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    streamWriter.WriteLine("<resources>");
                    streamWriter.WriteLine("</resources>");
                    streamWriter.Close();
                }

                // Add resource file to VS project
                var resourceFile = Path.Combine("Resources", folderAndFileName);
                CsProjHelper.IncludeFile(taxiHailProjectPath, resourceFile, "AndroidResource");
            }
        }
    }
}
