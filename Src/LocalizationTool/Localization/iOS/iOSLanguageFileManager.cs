using System.IO;

namespace apcurium.Tools.Localization.iOS
{
// ReSharper disable once InconsistentNaming
    public static class iOSLanguageFileManager
    {
        public static void CreateResourceFileIfNecessary(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return;
            }

            const string resourcesBasePath = @"..\Mobile\iOS";

            string taxiHailProjectPath = string.Format(@"{0}\TaxiHail.csproj", resourcesBasePath);
            string folderAndFileName = string.Format(@"{0}.lproj\Localizable.strings", language);
            string languageFileName = string.Format(@"{0}\{1}", resourcesBasePath, folderAndFileName);

            if (!File.Exists(languageFileName))
            {
                Directory.CreateDirectory(string.Format(@"{0}\{1}.lproj", resourcesBasePath, language));
                var stringFile = File.Create(languageFileName);
                stringFile.Close();

                // Add resource file to VS project
                CsProjHelper.IncludeFile(taxiHailProjectPath, folderAndFileName, "BundleResource");
            }
        }
    }
}
