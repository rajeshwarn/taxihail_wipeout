param([string]$env = "Dev")
Import-Module -Name WebAdministration #see cmdlets http://technet.microsoft.com/en-us/library/ee790599.aspx
$scriptpath = $MyInvocation.MyCommand.Path
$base_dir = Split-Path $scriptpath

Write-Host "***************Base Directory $base_dir *************************"
. $base_dir\deploy.$env.ps1
$dbTool = "$dbtoolPath\DatabaseInitializer.exe"

$connString =  "Data Source=.;Initial Catalog=$companyName;Integrated Security=True; MultipleActiveResultSets=True"

$existingWebApp = Get-WebApplication -Name $companyName
if($existingWebApp)
{
    Write-Host "***************Stop WebApp Pool $companyName ************************"  
    Stop-WebAppPool $companyName

}else{
   #need to create the app pool to get the IIS user to be addedin the sql server
   New-WebAppPool -Name $companyName
   Set-ItemProperty IIS:\AppPools\$companyName managedRuntimeVersion v4.0
   Stop-WebAppPool $companyName
}

if($deplyoDB -eq 'Y')
{
    Write-Host "***************Start Deploy DB ************************" 
    
    $psi = New-Object System.Diagnostics.ProcessStartInfo($dbTool, "$companyName `"$connString`" $actionDb $sqlServerInstance")
    $psi.WorkingDirectory = $dbtoolPath
    $process = [Diagnostics.Process]::Start($psi)
    $process.StartInfo.WindowStyle = 1 #hidden
    $process.WaitForExit()
    $exitcode = $process.ExitCode
    if($exitcode -eq -1)
    {
        Write-Host "***************ERROR IN DATABASE DEPLOYEMENT SEE Log File in the Programm directory, Exiting  ************************"
        exit $exitcode
    }
    Write-Host "***************End Deploy DB ************************" 
 }

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

Write-Host "***************Deployment $companyName finished ************************"