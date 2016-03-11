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

function UpdateBlackberryVersion ( $versionVal )
{

    $executingScriptDirectory =   get-scriptdirectory       

    $manifestPath = Join-Path  $executingScriptDirectory "..\Mobile\TaxiHail.BlackBerry\Properties\AndroidManifest.xml"


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

    $versionShort = Select-Xml -xml $xam  -Xpath "//key[text() = 'CFBundleShortVersionString']/following-sibling::string[1]" 
    $versionShort.Node."#text" = $versionVal
    
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
$r= [System.Text.RegularExpressions.Regex]::Match($args[0], "^[0-9]+(\.[0-9]+){1,3}$");
  
echo "Starting...";
if ($r.Success)
{
	# Enable the GitHub shell command
  Invoke-Expression "$env:LOCALAPPDATA\GitHub\shell.ps1";


	# validate tag does not already exist
		$tagExist = $(git tag -l "$args" 2>&1);
		if($tagExist)
	  {
		  echo " ";
		  echo "ERROR: TAG already exist!";
		  echo " ";
		  Exit;
	  }

	  echo " ";
	  echo "TAG OK";
	  echo " ";

	# get current git status
	$gitStatus= $(git status 2>&1);


	# Verify if in a valid GitHub repo
	$notRepo = Select-String -Pattern "fatal" -InputObject $gitStatus -Quiet;
  if ($notRepo)
  {
	  echo " ";
	  echo "ERROR: Not a valid repository";
	  echo " ";
	  Exit;
  }

	# Verify if there is uncommited change
	$uncommitedChange = Select-String -Pattern "Changes not staged for commit" -InputObject $gitStatus -Quiet;
  if ($uncommitedChange)
  {
	  echo " ";
	  echo "Current Branch: ";
	  $gitStatus | select -First 1;
	  echo " ";
	  echo "ERROR: You have uncommited change, please commit before doing a version release";
 	  echo " ";
	  Exit;
  }

	# Verify if there is no uncommited change
	$nothingToCommit = Select-String -Pattern "nothing to commit" -InputObject $gitStatus -Quiet;
  if ($nothingToCommit)
  {
	  echo " ";
	  echo "Current Branch: ";
	  $gitStatus | select -First 1;
	  echo "All change commited";
	  echo " ";

		echo "Inserting new version...";
	  Update-AllAssemblyInfoFiles $args[0];
	  UpdateAndroidVersion $args[0];
	  UpdateBlackberryVersion $args[0];
	  UpdateCallboxVersion $args[0];
	  UpdateIosVersion $args[0];
	  Add-Content ..\..\tagsList ($args[0])
	  echo " ";

		echo "Commiting change...";
		git commit -a -m "Version Bump $args"
		git tag "$args" 
	  echo " ";

		echo "Pushing to server...";
		git push
		git push origin "$args"
	  echo " ";

		echo "Updating customer portal list...";
		$UpdatePortalList = Invoke-WebRequest https://customer.taxihail.com/Admin/Deployment/UpdateVersions?createType=1
		echo $UpdatePortalList
		$PortalUpdated = Select-String -Pattern "StatusCode        : 200" -InputObject $UpdatePortalList -Quiet;
		if($PortalUpdated)
	  {
		  echo "Customer Portal list updated";
	  }
	  else
	  {
		  echo "ERROR: Unable to update customer portal list";
	  }

	  echo " ";
		echo "Done!";
	  echo " ";
  }

	# Something is wrong with the GitHub status
	else
	{
	  echo " ";
	  echo "Unexpected error!"
	  echo " ";
	  git status;
	  echo " ";
	  Exit ;
	}
}
else
{
  echo " ";
  echo "Bad Input!"
  echo " ";
  Usage ;
}
