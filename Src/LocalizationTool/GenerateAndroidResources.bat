echo Generate String Resources
output\LocalizationTool.exe -t=android -m="..\Mobile\Common\Localization\Master.resx" -d="..\Mobile\Android\Resources\Values\String.xml" -s="..\Mobile\Common\Settings\Settings.json"
output\LocalizationTool.exe -t=android -m="..\Mobile\Common\Localization\Master.ar.resx" -d="..\Mobile\Android\Resources\Values-ar\String.xml" -s="..\Mobile\Common\Settings\Settings.json"
output\LocalizationTool.exe -t=android -m="..\Mobile\Common\Localization\Master.fr.resx" -d="..\Mobile\Android\Resources\Values-fr\String.xml" -s="..\Mobile\Common\Settings\Settings.json"

PAUSE