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
                // English will always already be created
                return;
            }

            string resourcesBasePath = Path.GetFullPath(Path.Combine("..", "Mobile", "iOS"));
            string taxiHailProjectPath = Path.Combine(resourcesBasePath, "TaxiHail.csproj");

            string folderAndFileName = Path.Combine(string.Format("{0}.lproj", language), "Localizable.strings");
            string languageFileName = Path.Combine(resourcesBasePath, folderAndFileName);

            if (!File.Exists(languageFileName))
            {
                Directory.CreateDirectory(Path.Combine(resourcesBasePath, string.Format("{0}.lproj", language)));
                var stringFile = File.Create(languageFileName);
                stringFile.Close();

                // Add resource file to VS project
                CsProjHelper.IncludeFile(taxiHailProjectPath, folderAndFileName, "BundleResource");
            }
        }
    }
}
