properties{
    #$newVersion = "1.4.0.1"
    $newVersion = $ReleaseVersion
}

properties{#directories
    #$framework_dir_2 = "$env:systemroot\microsoft.net\framework\v2.0.50727"
    #$framework_dir_3_5 = "$env:systemroot\microsoft.net\framework\v3.5"
    $base_dir = [System.IO.Directory]::GetParent("$pwd")
    $build_dir = "$base_dir\build"
}

properties { #solution file
    $sln_file_2005 = "$base_dir\vs2008_bidshelper.sln"
    $sln_file_2008 = "$base_dir\SQL2008_bidshelper.sln"
    #$sql_ver = "2005"
}


properties { #utility locations
    $msbuild = "msbuild.exe"  
    $tf = "$env:ProgramFiles\Microsoft Visual Studio 9.0\Common7\IDE\tf.exe"
    $nsisPath = "$env:ProgramFiles\NSIS\makensis.exe"
    $zip = "$env:ProgramFiles\7-zip\7z.exe"
}


properties { #version files
    $versionFiles = @("$base_dir\assemblyinfo.cs", "ascii"),
        @("$base_dir\setupScript\BIDSHelperSetup.nsi", "ascii"),
        @("$base_dir\setupScript\BIDSHelperSetup2008.nsi", "ascii"),
        @("$base_dir\setupScript\SQL2005CurrentReleaseVersion.xml", "UTF8"),
        @("$base_dir\setupScript\SQL2008CurrentReleaseVersion.xml", "UTF8"),
        @("$base_dir\BIDSHelper.Addin", "unicode"),
        @("$base_dir\BIDSHelper2008.Addin", "unicode")
    $lastReleaseFile = "$base_dir\setupScript\SQL2005CurrentReleaseVersion.xml"
}


task default -depends CheckinVersionFiles

task Clean{ 
    Write-Host "Executed Clean!"
}

task checkVersion {
    if($newVersion.Length -eq 0)
    {
        # if we have not been given an explicit build, then we increment the build number
        # of the current release by 1.
        [string]$(get-Content "$($lastReleaseFile)") -cmatch '\d+\.\d+\.\d+\.\d+'
        $ver = $matches[0]
        $newVersion = $ver -replace "(?'main'\d+\.\d+\.\d+\.)(?'build'\d+)", "$($matches['main'])$([int]$matches['build']+1)"
    }
    
    
}

task updateVersion -depends checkVersion {
    write-Host $newVersion
    
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
        $newContent =  $(get-Content "$($_[0])") -replace "\d+\.\d+\.\d+\.\d+", $newVersion
        set-Content $_[0] $newContent -encoding $_[1]
    }
}



task Compile -depends updateVersion, Clean {
    write-Host "Path: $msbuild"
    &($msbuild) $sln_file_2005 /t:Rebuild /p:Configuration=Release /v:q
    Write-Host "Executed Compile!"
}

task BuildSetup -depends Compile {
    write-Host "Starting NSIS"
    &($nsisPath) "$base_dir\SetupScript\BIDSHelperSetup.nsi"
    write-Host "Completed NSIS"
}

task BuildXCopy -depends Compile {
    write-Host "Starting Zip"
    $ver = $newVersion -replace "\.", "_"
    &($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\bin\BIDSHelper.dll"
    &($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2005_$ver.zip" "$base_dir\BIDSHelper.Addin"
}

task Compile2008 -depends updateVersion, Clean, BuildXCopy,BuildSetup { # the 2008 release depends on the 2005 release so they get built sequentially
write-Host "Path:  $msbuild"
    &($msbuild) $sln_file_2008 /t:Rebuild /p:Configuration=Release /v:q
    Write-Host "Executed Compile!"
}

task BuildSetup2008 -depends Compile2008 {
    write-Host "Starting NSIS"
    &($nsisPath) "$base_dir\SetupScript\BIDSHelperSetup2008.nsi"
    write-Host "Completed NSIS"
}

task BuildXCopy2008 -depends Compile2008 {
    write-Host "Starting Zip"
    $ver = $newVersion -replace "\.", "_"
    &($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\bin\BIDSHelper.dll"
    &($zip) a -tzip "$base_dir\SetupScript\BIDSHelper2008_$ver.zip" "$base_dir\BIDSHelper2008.Addin"
}


task CheckinVersionFiles -depends BuildSetup, BuildXCopy, BuildSetup2008, BuildXCopy2008 {
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
            &($tf) checkin "/login:$user,$pass" "/comment:`"Updating to version $newVersion`"" "$($_[0])"
            write-host ($tf) checkin "/login:$user,$pass" "/comment:`"Updating to version $newVersion`"" "$($_[0])"
        }   
        &($tf) label "`"Release $newVersion`"" "/login:$user,$pass" /v:T /recursive *.*
    }
    else
    {
        write-Host "No Files checked-in"
    }
}