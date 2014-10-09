using System.IO;

namespace apcurium.Tools.Localization.Android
{
    public static class AndroidLanguageFileManager
    {
        public static void CreateResourceFileIfNecessary(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return;
            }

            const string resourcesBasePath = @"..\Mobile\Android\Resources";
            const string taxiHailProjectPath = @"..\Mobile\Android\TaxiHail.csproj";

            string folderAndFileName = string.Format(@"values-{0}\String.xml", language);
            string languageFileName = string.Format(@"{0}\{1}", resourcesBasePath, folderAndFileName);

            if (!File.Exists(languageFileName))
            {
                Directory.CreateDirectory(string.Format(@"{0}\values-{1}", resourcesBasePath, language));
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
                var resourceFile = string.Format(@"Resources\{0}", folderAndFileName);
                CsProjHelper.IncludeFile(taxiHailProjectPath, resourceFile, "AndroidResource");
            }
        }
    }
}
