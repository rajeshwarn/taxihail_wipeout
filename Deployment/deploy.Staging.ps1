$companyName= "MkWebStaging"
$sqlServerInstance = "MSSQL10_50.MSSQLSERVER"
$deployWwwRoot = "C:\Data\TaxiHail"
$version = "1.0" + [DateTime]::Now.Ticks #todo replace with tag...
$dbtoolPath = "C:\Data\teamcity\server\buildAgent\work\ada9756f55920a09\Src\Server\DatabaseInitializer\bin\Staging"
$websiteFiles = "C:\Data\teamcity\server\buildAgent\work\ada9756f55920a09\Src\Server\apcurium.MK.Web\obj\Staging\Package\PackageTmp\"
$site = 'Default Web Site'
$deplyoDB = 'Y'
$actionDb = "C" #C = create / U= Update
$deplyoWebsite = 'Y'