#see staging file for documentation
$companyName= "TaxiHailDemoUK" 

$sqlServerInstance = "MSSQL10_50.MSSQLSERVER"

$deployWwwRoot = "C:\Data\TaxiHail Sites"

$version = "1.1" + [DateTime]::Now.Ticks #todo replace with tag...

$dbtoolPath = "$base_dir\DatabaseInitializer"

$websiteFiles = "$base_dir\WebSites\"

$site = 'Default Web Site'

$deplyoDB = 'Y'

$actionDb = "U"

$deplyoWebsite = 'Y'