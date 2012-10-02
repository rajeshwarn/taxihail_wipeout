Import-Module -Name WebAdministration #see cmdlets http://technet.microsoft.com/en-us/library/ee790599.aspx
$scriptpath = $MyInvocation.MyCommand.Path
$base_dir = Split-Path $scriptpath

Write-Host "***************Base Directory $base_dir *************************"
#. $base_dir'\deployProps.ps1'
$companyName= "MyCompany"
$dbtoolPath = "$base_dir\Database"
$dbTool = "$dbtoolPath\DatabaseInitializer.exe" 
$sqlServerInstance = "MSSQL11.MSSQLSERVER"

$websiteFiles = "$base_dir\Website\"
$deployWwwRoot = "C:\Data\TaxiHail"
$site = 'Default Web Site'
$connString =  "Data Source=.;Initial Catalog=$companyName;Integrated Security=True; MultipleActiveResultSets=True"

$version = Read-Host "Version ?"

$existingWebApp = Get-WebApplication -Name $companyName
if($existingWebApp)
{
    Write-Host "***************Stop WebApp Pool $companyName ************************"  
    Stop-WebAppPool $companyName

}else{
   #need to create the app pool to get the IIS user to be addedin the sql server
   New-WebAppPool -Name $companyName
   Stop-WebAppPool $companyName
}

$deplyoDB = Read-Host "Deploy Database? Y/N"
if($deplyoDB -eq 'Y')
{
    Write-Host "***************Start Deploy DB ************************" 
    $actionDb = Read-Host "[C]reate (with delete existing) or [U]pdate database? C/U"

    $psi = New-Object System.Diagnostics.ProcessStartInfo($dbTool, "$companyName `"$connString`" $actionDb $sqlServerInstance")
    $psi.WorkingDirectory = $dbtoolPath
    $process = [Diagnostics.Process]::Start($psi)
    $process.WaitForExit()
    Write-Host "***************End Deploy DB ************************" 
 }

 $deplyoWebsite = Read-Host "Deploy WebSite? Y/N"
 $targetDir = "$deployWwwRoot\$companyname\$version"
if($deplyoWebsite -eq 'Y')
{
    Write-Host "***************Start Deploy WebSite ************************"     
    #create dir and copy files
    New-Item $targetDir -type directory    
    Get-ChildItem $websiteFiles -Recurse | Copy-Item -Filter "*.*" -Force -Destination {Join-Path $targetDir $_.FullName.Substring($websiteFiles.length)} 
    #change IIS configuration  
    if($existingWebApp)
    {
        Remove-WebApplication -Site $site  -Name $companyName
    }
    $existingWebApp = New-WebApplication -Site $site  -Name $companyName -PhysicalPath $targetDir -ApplicationPool $companyName
    #modify connectionstring in web.config
    # Get file + cast as xml
    $xml = [xml](get-content "$targetDir\Web.config")     
    $root = $xml.get_DocumentElement();
    # Change existing node
    $root.connectionStrings.add.connectionString = $connString
    $xml.Save("$targetDir\Web.config")
    Write-Host "***************End Deploy WebSite ************************"
}

Write-Host "***************Start Pool $companyName ************************"
Start-WebAppPool -Name $companyName