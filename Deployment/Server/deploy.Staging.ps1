$companyName= "MkWebStaging" #use for web app and pool name, db name, folder name

$sqlServerInstance = "MSSQL10_50.MSSQLSERVER" #instance name, to have it copy the name of this folder C:\Program Files\Microosft SQl Server\<MSSQLXX.MSSQLSERVER> 

$version = "1.0" + [DateTime]::Now.Ticks #version / todo replace with tag...

$dbtoolPath = "C:\Data\teamcity\server\buildAgent\work\ada9756f55920a09\Src\Server\DatabaseInitializer\bin\Debug" #path to the DatabaseInitiliazer.exe, you can use $base_dir which is the current dir

$websiteFiles = "C:\Data\teamcity\server\buildAgent\work\ada9756f55920a09\Src\Server\apcurium.MK.Web\obj\Release\Package\PackageTmp\" #path to the webapp files to be deployed

$deployWwwRoot = "C:\Data\TaxiHail" #where the web app will be deployed

$site = 'Default Web Site' #name of the website in IIS

$deplyoDB = 'Y' #deploy database ? Y = Yes / N = No

$actionDb = "C" #database will be C = created / U= updated

$deplyoWebsite = 'Y' #deploy webapp ? Y = Yes / N = No