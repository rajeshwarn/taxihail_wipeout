using System.IO;

namespace apcurium.Tools.Localization.iOS
{
// ReSharper disable once InconsistentNaming
    public static class iOSLanguageFileManager
    {
        public static void CreateResourceFileIfNecessary(string language)
        {
            var resourcesBasePath = Path.GetFullPath(Path.Combine("..", "Mobile", "iOS"));
            var taxiHailProjectPath = Path.Combine(resourcesBasePath, "TaxiHail.csproj");
            var folderAndFileName = Path.Combine(string.Format("{0}.lproj", language), "Localizable.strings");
            var englishFolderAndFileName = Path.Combine("en.lproj", "Localizable.strings");

            if (string.IsNullOrEmpty(language))
            {
                // English will always already be created

                // Clear file
                var englishLanguageFileName = Path.Combine(resourcesBasePath, englishFolderAndFileName);
                File.WriteAllText(englishLanguageFileName, string.Empty);
                return;
            }

            string languageFileName = Path.Combine(resourcesBasePath, folderAndFileName);

            if (!File.Exists(languageFileName))
            {
                Directory.CreateDirectory(Path.Combine(resourcesBasePath, string.Format("{0}.lproj", language)));
                var stringFile = File.Create(languageFileName);
                stringFile.Close();

                // Add resource file to VS project
                CsProjHelper.IncludeFile(taxiHailProjectPath, folderAndFileName, "BundleResource");
            }
            else
            {
                // Clear file
                File.WriteAllText(languageFileName, string.Empty);
            }
        }
    }
}
