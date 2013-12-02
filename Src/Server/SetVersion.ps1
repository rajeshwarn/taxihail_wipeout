# SetVersion.ps1
#
# Set the version in all the AssemblyInfo.cs or AssemblyInfo.vb files in any subdirectory.
#
# usage:  
#  from cmd.exe: 
#     powershell.exe SetVersion.ps1  2.8.3.0
# 
#  from powershell.exe prompt: 
#     .\SetVersion.ps1  2.8.3.0
#
# last saved Time-stamp: <Wednesday, April 23, 2008  11:52:15  (by dinoch)>
#


function Usage
{
  echo "Usage: ";
  echo "  from cmd.exe: ";
  echo "     powershell.exe SetVersion.ps1  2.8.3.0";
  echo " ";
  echo "  from powershell.exe prompt: ";
  echo "     .\SetVersion.ps1  2.8.3.0";
  echo " ";
}


function Update-SourceVersion
{
  Param ([string]$Version)
  $NewVersion = 'AssemblyVersion("' + $Version + '")';
  $NewFileVersion = 'AssemblyFileVersion("' + $Version + '")';



  foreach ($o in $input) 
  {
    Write-output $o.FullName
    $TmpFile = $o.FullName + ".tmp"
     get-content $o.FullName | 
        %{$_ -replace 'AssemblyVersion\(".*"\)', $NewVersion } |
        %{$_ -replace 'AssemblyFileVersion\(".*"\)', $NewFileVersion } |  Out-File $TmpFile -encoding "UTF8";

     move-item $TmpFile $o.FullName -force
  }
}


function Update-AllAssemblyInfoFiles ( $version )
{
  foreach ($file in "AssemblyInfo.cs", "AssemblyInfo.vb" ) 
  {
    get-childitem -recurse | Where-Object { $_.FullName -notmatch "\bInfrastructure\b" } |? {$_.Name -eq $file} | Update-SourceVersion $version ;
  }
}


function UpdateAndroidVersion ( $versionVal )
{

    $executingScriptDirectory =   get-scriptdirectory       

    $manifestPath = Join-Path  $executingScriptDirectory "..\Mobile\Android\Properties\AndroidManifest.xml"


    # Load the bootstrap file
    [xml] $xam = Get-Content -Path ($manifestPath)
    
    # Get the version from Android Manifest
    $version = Select-Xml -xml $xam  -Xpath "/manifest/@android:versionName" -namespace @{android="http://schemas.android.com/apk/res/android"}
        
    $version.Node.Value = $versionVal

    $versionCode = Select-Xml -xml $xam  -Xpath "/manifest/@android:versionCode" -namespace @{android="http://schemas.android.com/apk/res/android"}
    
    [int] $iVer  = $versionCode.Node.Value

    $versionCode.Node.Value = ($iVer + 1)
 
    # Save the file
    $xam.Save( $manifestPath)
}

function UpdateCallboxVersion ( $versionVal )
{

    $executingScriptDirectory =   get-scriptdirectory       

    $manifestPath = Join-Path  $executingScriptDirectory "..\Mobile\MK.Callbox.Mobile.Client.Android\Properties\AndroidManifest.xml"


    # Load the bootstrap file
    [xml] $xam = Get-Content -Path ($manifestPath)
    
    # Get the version from Android Manifest
    $version = Select-Xml -xml $xam  -Xpath "/manifest/@android:versionName" -namespace @{android="http://schemas.android.com/apk/res/android"}
        
    $version.Node.Value = $versionVal

    $versionCode = Select-Xml -xml $xam  -Xpath "/manifest/@android:versionCode" -namespace @{android="http://schemas.android.com/apk/res/android"}
    
    [int] $iVer  = $versionCode.Node.Value

    $versionCode.Node.Value = ($iVer + 1)
 
    # Save the file
    $xam.Save( $manifestPath)
}



function UpdateIosVersion ( $versionVal )
{

    $executingScriptDirectory =   get-scriptdirectory       

    $plistPath = Join-Path  $executingScriptDirectory "..\Mobile\iOS\Info.plist"
    # Load the bootstrap file
    [xml] $xam = Get-Content -Path ( $plistPath )
    
    # Get the version from Android Manifest
    $version = Select-Xml -xml $xam  -Xpath "//key[text() = 'CFBundleVersion']/following-sibling::string[1]" 
        #//key[text() = 'CFBundleVersion']/following-sibling::string
        #//key[ CFBundleVersion( following-sibling::*[1] ) != 'key' ]
    #$a = $version.Node."#text";
    $version.Node."#text" = $versionVal 
    
    
    # Save the file
    $xam.Save( $plistPath )
}

function get-scriptdirectory {
 
# .SYNOPSIS 
#   Return the current script directory path, compatible with PrimalScript 2009
#   Equivalent to VBscript fso.GetParentFolderName(WScript.ScriptFullName)
#   Requires PowerShell 2.0
#    
# .DESCRIPTION
#   Author   : Jean-Pierre.Paradis@fsa.ulaval.ca
#   Date     : March 31, 2010
#   Version  : 1.01
#
# .LINK 
#   http://blog.sapien.com/index.php/2009/09/02/powershell-hosting-and-myinvocation/
 
    if (Test-Path variable:\hostinvocation) 
        {$FullPath=$hostinvocation.MyCommand.Path}
    Else {
        $FullPath=(get-variable myinvocation -scope script).value.Mycommand.Definition }    
    if (Test-Path $FullPath) {
        return (Split-Path $FullPath) 
        }
    Else {
        $FullPath=(Get-Location).path
        Write-Warning ("Get-ScriptDirectory: Powershell Host <" + $Host.name + "> may not be compatible with this function, the current directory <" + $FullPath + "> will be used.")
        return $FullPath
        }
}

# validate arguments 
#$r= [System.Text.RegularExpressions.Regex]::Match($args[0], "^[0-9]+(\.[0-9]+){1,3}$");
  
 # echo "Starting...";
#if ($r.Success)
#{
  
  Update-AllAssemblyInfoFiles $args[0];
  UpdateAndroidVersion $args[0];
  UpdateCallboxVersion $args[0];
  UpdateIosVersion $args[0];
  
#}
#else
#{
#  echo " ";
#  echo "Bad Input!"
#  echo " ";
#  Usage ;
#}
