echo Generate String Resources
output\LocalizationTool.exe -t=android -m="..\Mobile\Common\Localization\Master.resx" -d="..\Mobile\Android\Resources\Values\String.xml" -s="..\Mobile\Common\Settings\Settings.json"
output\LocalizationTool.exe -t=ios -m="..\Mobile\Common\Localization\Master.resx" -d="..\Mobile\iOS\en.lproj\localizable.strings" -s="..\Mobile\Common\Settings\Settings.json"
output\LocalizationTool.exe -t=callbox -m="..\Mobile\Common\Localization\Master.resx" -d="..\Mobile\MK.Callbox.Mobile.Client.Android\Resources\Values\Strings.xml" -s="..\Mobile\Common\Settings\Settings.json"
PAUSE