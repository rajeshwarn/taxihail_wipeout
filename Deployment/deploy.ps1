$scriptpath = $MyInvocation.MyCommand.Path
$base_dir = Split-Path $scriptpath
Write-Host "***************Base Directory $base_dir *************************"
. $base_dir'\deployProps.ps1'

Write-Host "***************Stop Website $companyName ************************"
Import-Module -Name WebAdministration
Stop-WebSite -Name $companyName


$actionDb = Read-Host "[C]reate (with delete existing) or [U]pdate database ? C/U"

$psi = New-Object System.Diagnostics.ProcessStartInfo($dbTool, "$companyName $actionDb")
$psi.FileName = $dbTool
$psi.Arguments = "$companyName $actionDb"
$psi.WorkingDirectory = $dbtoolPath
$process = [Diagnostics.Process]::Start($psi)
$process.WaitForExit()


Write-Host "***************Start Website $companyName ************************"
Start-WebSite -Name $companyName