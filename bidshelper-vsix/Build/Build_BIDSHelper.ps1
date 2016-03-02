param(
    [string]$ReleaseVersion = ""
)

#IntPtr in 64bit is 8 bytes
function is64bit() {    if ([IntPtr].Size -eq 4) { return $false }    else { return $true }}

#$framework_dir_2 = "$env:systemroot\microsoft.net\framework\v2.0.50727"
$framework_dir_3_5 = "$env:systemroot\microsoft.net\framework\v3.5"
$base_dir = [System.IO.Directory]::GetParent("$pwd")
$build_dir = "$base_dir\build"
$sln_file_2005 = "$base_dir\vs2008_bidshelper.sln"
$sln_file_2008 = "$base_dir\SQL2008_bidshelper.sln"

#utility locations
$msbuild = "$framework_dir_3_5\msbuild.exe"  
$tf = "$env:ProgramFiles\Microsoft Visual Studio 9.0\Common7\IDE\tf.exe"
$nsisPath = "$env:ProgramFiles\NSIS\makensis.exe"

if(is64bit -eq $true) 
{
$tf = "$env:ProgramFiles (x86)\Microsoft Visual Studio 9.0\Common7\IDE\tf.exe"
$nsisPath = "$env:ProgramFiles (x86)\NSIS\makensis.exe"
}


$zip = "$env:ProgramFiles\7-zip\7z.exe"

#version files
$versionFiles = @("$base_dir\Properties\AssemblyInfo.cs", "ascii"),
        @("$base_dir\setupScript\BIDSHelperSetup.nsi", "ascii"),
        @("$base_dir\setupScript\BIDSHelperSetup2008.nsi", "ascii"),
        @("$base_dir\setupScript\SQL2005CurrentReleaseVersion.xml", "UTF8"),
        @("$base_dir\setupScript\SQL2008CurrentReleaseVersion.xml", "UTF8"),
        @("$base_dir\BIDSHelper.Addin", "unicode"),
        @("$base_dir\BIDSHelper2008.Addin", "unicode")
$lastReleaseFile = "$base_dir\setupScript\SQL2005CurrentReleaseVersion.xml"

#Clean 

#checkVersion 
    if($ReleaseVersion.Length -eq 0)
    {
        # if we have not been given an explicit build, then we increment the build number
        # of the current release by 1.
        [string]$(get-Content "$($lastReleaseFile)") -cmatch '\d+\.\d+\.\d+\.\d+'
        $ver = $matches[0]
        
   		# HACK: having this regex match appears to make the replace statement work
		# removing it makes the following replace line fail for some reason.
        $null = $ver -cmatch "(?'main'\d+\.\d+\.\d+\.)(?'build'\d+)"
        
        $ReleaseVersion = $ver -replace "(?'main'\d+\.\d+\.\d+\.)(?'build'\d+)", "$($matches['main'])$([int]$matches['build']+1)"
    }
    
# updateVersion 
    write-Host $ReleaseVersion
    
    $user = read-Host "Codeplex UserName"
    $pass =  read-Host "Codeplex Password" -assecurestring
    
    # I'm only using secure string to hide the password on screen, so we convert it
    # back to a regular string so that it can be passed to codeplex
    $BasicString = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($pass)
    $pass = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BasicString)
    
    #get the lastest version from codeplex
    &($tf) get "`$/bidshelper" "/login:$user,$pass" *.* /v:T /recursive
    
    #checkout the files with version information
    $versionFiles | foreach-Object {
        write-Host "checking out: $($_[0])"
        &($tf) checkout "/login:$user,$pass" $_[0]
    }
    
    $versionFiles | foreach-Object {
        $newContent =  $(get-Content "$($_[0])") -replace "\d+\.\d+\.\d+\.\d+", $ReleaseVersion
        set-Content $_[0] $newContent -encoding $_[1]
    }

#Compile
    write-Host "Path: $msbuild"
    &($msbuild) $sln_file_2005 /t:Rebuild /p:Configuration=Release /v:q
    Write-Host "Executed Compile!"

#BuildSetup 
    write-Host "Starting NSIS"
    &($nsisPath) "$base_dir\SetupScript\BIDSHelperSetup.nsi"
    write-Host "Completed NSIS"

#BuildXCopy 
    write-Host "Starting Zip"
    $ver = $ReleaseVersion -replace "\.", "_"
    &($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\bin\BIDSHelper.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\bin\Antlr3.Runtime.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\bin\BimlEngine.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\bin\PostSharp.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\bin\ExpressionEditor.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\bin\DLLs\Biml\Biml.xsd"
    &($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\BIDSHelper.Addin"

#Compile2008 
write-Host "Path:  $msbuild"
    &($msbuild) $sln_file_2008 /t:Rebuild /p:Configuration=Release /v:q
    Write-Host "Executed Compile!"

#BuildSetup2008
    write-Host "Starting NSIS"
    &($nsisPath) "$base_dir\SetupScript\BIDSHelperSetup2008.nsi"
    write-Host "Completed NSIS"

#BuildXCopy2008
    write-Host "Starting Zip"
    $ver = $ReleaseVersion -replace "\.", "_"
    &($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\bin\BIDSHelper.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\bin\Antlr3.Runtime.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\bin\BimlEngine.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\bin\PostSharp.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\bin\ExpressionEditor.dll"
	&($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\bin\DLLs\Biml\Biml.xsd"
    &($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\BIDSHelper2008.Addin"

# CheckinVersionFiles
    $cont = read-host "
Release Files Built!    
====================

Upload the files to codeplex now.

Hit 'Y' and enter to continue and checkin and label the files on codeplex. 
Just hit Enter to abort without updating the source repository."
    write-Host $cont
    if ($cont -match "^Y$")
    {
        write-Host "checking files into codeplex and applying release label"
        $versionFiles | foreach-Object {
            write-Host "checking out: $($_[0])"
            &($tf) checkin "/login:$user,$pass" "$($_[0])" "/comment:`"Updating to version $ReleaseVersion`""
        }   
        &($tf) label "`"Release $ReleaseVersion`"" *.* "/login:$user,$pass" /v:T /recursive
    }
    else
    {
        write-Host "No Files checked-in"
    }