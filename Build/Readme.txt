BIDSHelper - PowerShell Build Environment
=========================================

Author: Darren Gosbell
Date  : 6 March 2009

To run a build simply type the following at the command line.

powershell .\build_BIDSHelper.ps1 [-ReleaseVersion=<version>]

You will be prompted for your codeplex username/password 
and once everything is built you will be asked to upload the files
to codeplex and then you have the option of checking in the files
that were checked out. As soon as you check the files in the
version check feature will be able to "see" the new release.

You can either run the build with or without a specific version number.

eg.

powershell .\build_BIDSHelper.ps1 -ReleaseVersion="1.5.0.0"

powershell .\build_BIDSHelper.ps1

If you leave off the build number the script will take the number
from the SQL2005CurrentReleaseVersion.xml file and increment the
last digit by one.