Import-Module -Name WebAdministration #see cmdlets http://technet.microsoft.com/en-us/library/ee790599.aspx
$scriptpath = $MyInvocation.MyCommand.Path
$base_dir = Split-Path $scriptpath
Write-Host "***************Base Directory $base_dir *************************"
#. $base_dir'\deployProps.ps1'
$companyName= "MyCompany"
$dbtoolPath = "$base_dir\DatabaseInitializer\"
$dbTool = "$dbtoolPath\DatabaseInitializer.exe" 

$websiteFiles = "$base_dir\Website\"
$deployWwwRoot = "C:\Data\TaxiHail"
$site = 'Default Web Site'

$version = Read-Host "Version ?"

$existingWebApp = Get-WebApplication -Name $companyName
if($existingWebApp)
{
    Write-Host "***************Stop WebApp Pool $companyName ************************"  
    Stop-WebAppPool $companyName
}
  


$deplyoDB = Read-Host "Deploy Database? Y/N"
if($deplyoDB -eq 'Y')
{
    Write-Host "***************Start Deploy DB ************************" 
    $actionDb = Read-Host "[C]reate (with delete existing) or [U]pdate database? C/U"

    $psi = New-Object System.Diagnostics.ProcessStartInfo($dbTool, "$companyName $actionDb")
    $psi.FileName = $dbTool
    $psi.Arguments = "$companyName $actionDb"
    $psi.WorkingDirectory = $dbtoolPath
    $process = [Diagnostics.Process]::Start($psi)
    $process.WaitForExit()
    Write-Host "***************End Deploy DB ************************" 
 }

 $deplyoWebsite = Read-Host "Deploy WebSite? Y/N"
if($deplyoWebsite -eq 'Y')
{
    Write-Host "***************Start Deploy WebSite ************************" 
    $targetDir = "$deployWwwRoot\$companyname\$version"
    #create dir and copy files
    New-Item $targetDir -type directory    
    Get-ChildItem $websiteFiles -Recurse | Copy-Item -Filter "*.*" -Force -Destination {Join-Path $targetDir $_.FullName.Substring($websiteFiles.length)} 
    #change IIS configuration  
    if($existingWebApp)
    {
        Remove-WebApplication -Site $site  -Name $companyName
        New-WebApplication -Site $site  -Name $companyName -PhysicalPath $targetDir -ApplicationPool $companyName        

    }else{

        $appPool = New-WebAppPool -Name $companyName
        $existingWebApp = New-WebApplication -Site $site  -Name $companyName -PhysicalPath $targetDir -ApplicationPool $appPool.Name
    }
    Write-Host "***************End Deploy WebSite ************************"
}





Write-Host "***************Start Website $companyName ************************"
Start-WebAppPool -Name $companyName