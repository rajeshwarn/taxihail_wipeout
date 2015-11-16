using System.IO;

namespace apcurium.Tools.Localization.Android
{
    public static class AndroidLanguageFileManager
    {
		public static void CreateAndroidClientResourceFileIfNecessary(string language)
		{
			CreateResourceFileIfNecessary(language);
		}

		public static void CreateCallboxClientResourceFileIfNecessary(string language)
		{
			CreateResourceFileIfNecessary (language, "MK.Callbox.Mobile.Client.Android");
		}

		private static void CreateResourceFileIfNecessary(string language, string projectFolder = "Android")
        {
			var resourcesBasePath = Path.GetFullPath(Path.Combine("..", "Mobile", projectFolder, "Resources"));
            var taxiHailProjectPath = Path.GetFullPath(Path.Combine("..", "Mobile", "Android", "TaxiHail.csproj"));
            var englishFolderAndFileName = Path.Combine("values", "String.xml");

            if (string.IsNullOrEmpty(language))
            {
                // English will always already be created

                var englishLanguageFileName = Path.Combine(resourcesBasePath, englishFolderAndFileName);
                ClearFile(englishLanguageFileName);
                return;
            }

            var folderAndFileName = Path.Combine(string.Format("values-{0}", language), "String.xml");
            var languageFileName = Path.Combine(resourcesBasePath, folderAndFileName);

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
            else
            {
                ClearFile(languageFileName);
            }
        }

        private static void ClearFile(string fileName)
        {
            // Clear file
            File.WriteAllText(fileName, string.Empty);

            using (var streamWriter = new StreamWriter(fileName))
            {
                streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                streamWriter.WriteLine("<resources>");
                streamWriter.WriteLine("</resources>");
                streamWriter.Close();
            }
        }
    }
}
