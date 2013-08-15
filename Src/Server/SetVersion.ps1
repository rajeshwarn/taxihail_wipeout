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
        %{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
        %{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion } |  Out-File $TmpFile -encoding "UTF8";

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
    # Load the bootstrap file
    [xml] $xam = Get-Content -Path ($ProjectPath + "..\Mobile\Android\Properties\AndroidManifest.xml")
    
    # Get the version from Android Manifest
    $version = Select-Xml -xml $xam  -Xpath "/manifest/@android:versionName" -namespace @{android="http://schemas.android.com/apk/res/android"}
        
    $version.Node.Value = $versionVal

    $versionCode = Select-Xml -xml $xam  -Xpath "/manifest/@android:versionCode" -namespace @{android="http://schemas.android.com/apk/res/android"}
    
    [int] $iVer  = $versionCode.Node.Value

    $versionCode.Node.Value = ($iVer + 1)
 
    # Save the file
    $xam.Save($ProjectPath + "..\Mobile\Android\Properties\AndroidManifest.xml")
}


function UpdateIosVersion ( $versionVal )
{
    # Load the bootstrap file
    [xml] $xam = Get-Content -Path ($ProjectPath + "..\Mobile\iOS\Info.plist")
    
    # Get the version from Android Manifest
    $version = Select-Xml -xml $xam  -Xpath "//key[text() = 'CFBundleVersion']/following-sibling::string[1]" 
        #//key[text() = 'CFBundleVersion']/following-sibling::string
        #//key[ CFBundleVersion( following-sibling::*[1] ) != 'key' ]
    #$a = $version.Node."#text";
    $version.Node."#text" = $versionVal 
    
    
    # Save the file
    $xam.Save($ProjectPath + "..\Mobile\iOS\Info.plist")
}

# validate arguments 
#$r= [System.Text.RegularExpressions.Regex]::Match($args[0], "^[0-9]+(\.[0-9]+){1,3}$");
  
 # echo "Starting...";
#if ($r.Success)
#{
  Update-AllAssemblyInfoFiles $args[0];
  UpdateAndroidVersion $args[0];
  UpdateIosVersion $args[0];
#}
#else
#{
#  echo " ";
#  echo "Bad Input!"
#  echo " ";
#  Usage ;
#}
