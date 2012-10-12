param([string]$env = "Dev")

$scriptpath = $MyInvocation.MyCommand.Path
$base_dir = Split-Path $scriptpath
$Configuration="Release"

Remove-Item -ErrorAction SilentlyContinue -Recurse -Force $base_dir\Package$env
Remove-Item -ErrorAction SilentlyContinue -Recurse -Force $base_dir\Package$env.zip
New-Item -ItemType directory $base_dir\Package$env
New-Item -ItemType directory $base_dir\Package$env\DatabaseInitializer
New-Item -ItemType directory $base_dir\Package$env\WebSites
New-Item  -ItemType file $base_dir\Package$env.zip

Write-Host "***************Copy scripts *************************"

Copy-Item $base_dir\deploy.ps1 $base_dir\Package$env\deploy.ps1
Copy-Item $base_dir\deploy.$env.ps1 $base_dir\Package$env\deploy.$env.ps1


$MsBuild = $env:systemroot + "\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe";
$rootDir = (get-item $base_dir).parent.parent.FullName
$SlnFilePath = "$rootDir\Src\Server\MKBooking.sln"

Write-Host "***************Building $SlnFilePath *************************"
 
$BuildArgs = @{
 FilePath = $MsBuild
 ArgumentList = $SlnFilePath, "/t:rebuild", ("/p:Configuration=" + $Configuration), "/v:minimal"
 Wait = $true
}
 
# Start the build
$p = Start-Process @BuildArgs
$p.WaitForExit()

Get-ChildItem $rootDir\Src\Server\DatabaseInitializer\bin\$Configuration -Recurse | Copy-Item -Filter "*.*" -Force -Destination {Join-Path $base_dir\Package$env\DatabaseInitializer $_.FullName.Substring("$rootDir\Src\Server\DatabaseInitializer\bin\$Configuration".length)} 

Write-Host "***************Building Website *************************"

$SlnFilePath = "$rootDir\Src\Server\apcurium.MK.Web\apcurium.MK.Web.csproj"
$BuildArgs = @{
 FilePath = $MsBuild
 ArgumentList = $SlnFilePath, "/t:Package", ("/p:Configuration=" + $Configuration), "/v:minimal"
 Wait = $true
}

# Start the build
$p = Start-Process @BuildArgs
$p.WaitForExit()

Get-ChildItem $rootDir\Src\Server\apcurium.MK.Web\obj\$Configuration\Package\PackageTmp\ -Recurse | Copy-Item -Filter "*.*" -Force -Destination {Join-Path $base_dir\Package$env\WebSites $_.FullName.Substring("$rootDir\Src\Server\apcurium.MK.Web\obj\$Configuration\Package\PackageTmp\".length)} 

#Write-Host "***************Compress Files *************************"
#$filename = "$base_dir\Package$env.zip"
#$ZipFile = (new-object -com shell.application).NameSpace($filename) 
#Get-ChildItem $base_dir\Package$env | foreach {$ZipFile.CopyHere($_.fullname)} 
