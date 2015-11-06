echo Generate String Resources
output\LocalizationTool.exe -t=android -m="..\Mobile\Common\Localization\Master.resx" -d="..\Mobile\Android\Resources\Values\String.xml" -s="..\Mobile\Common\Settings\Settings.json"
output\LocalizationTool.exe -t=android -m="..\Mobile\Common\Localization\Master.resx" -d="..\Mobile\TaxiHail.BlackBerry\Resources\Values\String.xml" -s="..\Mobile\Common\Settings\Settings.json"
output\LocalizationTool.exe -t=ios -m="..\Mobile\Common\Localization\Master.resx" -d="..\Mobile\iOS\en.lproj\localizable.strings" -s="..\Mobile\Common\Settings\Settings.json"

PAUSE