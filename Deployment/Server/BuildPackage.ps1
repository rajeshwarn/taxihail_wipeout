param([string]$env = "Dev")

$scriptpath = $MyInvocation.MyCommand.Path
$base_dir = Split-Path $scriptpath
$Configuration="Release"

. $base_dir/ZipFunctions.ps1

Remove-Item -ErrorAction SilentlyContinue -Recurse -Force $base_dir\Package$env
Remove-Item -ErrorAction SilentlyContinue -Recurse -Force $base_dir\Package$env.zip
New-Item -ItemType directory $base_dir\Package$env
New-Item -ItemType directory $base_dir\Package$env\DatabaseInitializer
New-Item -ItemType directory $base_dir\Package$env\WebSites

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
Start-Process @BuildArgs


Get-ChildItem $rootDir\Src\Server\DatabaseInitializer\bin\$Configuration -Recurse | Copy-Item -Filter "*.*" -Force -Destination {Join-Path $base_dir\Package$env\DatabaseInitializer $_.FullName.Substring("$rootDir\Src\Server\DatabaseInitializer\bin\$Configuration".length)} 

Write-Host "***************Building Website *************************"

$SlnFilePath = "$rootDir\Src\Server\apcurium.MK.Web\apcurium.MK.Web.csproj"
$BuildArgs = @{
 FilePath = $MsBuild
 ArgumentList = $SlnFilePath, "/t:Package", ("/p:Configuration=" + $Configuration), "/v:minimal"
 Wait = $true
}

# Start the build
Start-Process @BuildArgs

Get-ChildItem $rootDir\Src\Server\apcurium.MK.Web\obj\$Configuration\Package\PackageTmp\ -Recurse | Copy-Item -Filter "*.*" -Force -Destination {Join-Path $base_dir\Package$env\WebSites $_.FullName.Substring("$rootDir\Src\Server\apcurium.MK.Web\obj\$Configuration\Package\PackageTmp\".length)} 

#Write-Host "***************Compress Files *************************"
[IO.DirectoryInfo] $directory = Get-Item $base_dir\Package$env
ZipFolder $directory
