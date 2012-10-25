$scriptpath = $MyInvocation.MyCommand.Path
$base_dir = Split-Path $scriptpath
$Configuration="Release"

Remove-Item -ErrorAction SilentlyContinue -Recurse -Force $base_dir\Package
New-Item -ItemType directory $base_dir\Package
New-Item -ItemType directory $base_dir\Package\DatabaseInitializer
New-Item -ItemType directory $base_dir\Package\WebSites

Write-Host "***************Copy scripts *************************"

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