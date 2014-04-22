; Script generated by the HM NIS Edit Script Wizard.

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "BIDS Helper 2012"
!define PRODUCT_VERSION "1.6.6.0"
!define PRODUCT_PUBLISHER "BIDS Helper"
!define PRODUCT_WEB_SITE "http://www.codeplex.com/bidshelper"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"
!define PRODUCT_SETTINGS_KEY "Software\${PRODUCT_NAME}"
!define PRODUCT_SETTINGS_ROOT_KEY "HKCU"
!define VSLOOK_IN_FOLDERS "Software\Microsoft\VisualStudio\10.0\AutomationOptions\LookInFolders"
!define VSLOOK_IN_FOLDERS2 "Software\Microsoft\VisualStudio\11.0\AutomationOptions\LookInFolders"

!define MUI_FINISHPAGE_TEXT "${PRODUCT_NAME} ${PRODUCT_VERSION} has been installed on your computer.\r\nIt will be activated next time you start SQL Server Data Tools - Business Intelligence (SSDTBI)\r\n\r\nClick Finish to close this wizard."
!define MUI_FINISHPAGE_TEXT_LARGE ""

SetCompressor lzma

; MUI 1.67 compatible ------
!include "MUI.nsh"
!include Library.nsh
!include LogicLib.nsh


; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "..\BIDSHelper.ico" #"${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "..\BIDSHelper.ico" #"${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "BIDSHelperHeader.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "BIDSHelper2012.bmp"

!define MUI_FINISHPAGE_LINK "BIDSHelper Homepage"
!define MUI_FINISHPAGE_LINK_LOCATION "http://bidshelper.codeplex.com"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!define MUI_LICENSEPAGE_RADIOBUTTONS
!insertmacro MUI_PAGE_LICENSE "License.rtf"
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"

; Reserve files
!insertmacro MUI_RESERVEFILE_INSTALLOPTIONS

; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "BIDSHelper2012Setup(${PRODUCT_VERSION}).exe"
InstallDir "$PROGRAMFILES\BIDS Helper 2012"
InstallDirRegKey HKLM "${PRODUCT_SETTINGS_KEY}" "$INSTDIR"
ShowInstDetails show
ShowUnInstDetails show

Section "MainSection" SEC01

  Call CloseParentWithUserApproval

  #SetOutPath "$DOCUMENTS\Visual Studio 2008\Addins"
  SetOutPath $INSTDIR
  SetOverwrite ifnewer
  CreateDirectory "$INSTDIR"
  #  File "..\bin\BIDSHelper.dll"
  !define LIBRARY_IGNORE_VERSION
  !insertmacro InstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED "..\bin\BIDSHelper.dll" "$INSTDIR\BIDSHelper.dll" $INSTDIR\Temp
  !insertmacro InstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED "..\bin\Antlr3.Runtime.dll" "$INSTDIR\Antlr3.Runtime.dll" $INSTDIR\Temp
  !insertmacro InstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED "..\bin\BimlEngine.dll" "$INSTDIR\BimlEngine.dll" $INSTDIR\Temp
  !insertmacro InstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED "..\bin\ExpressionEditor.dll" "$INSTDIR\ExpressionEditor.dll" $INSTDIR\Temp
  !insertmacro InstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED "..\bin\PostSharp.dll" "$INSTDIR\PostSharp.dll" $INSTDIR\Temp
  !undef LIBRARY_IGNORE_VERSION
  File "..\BIDSHelper2012.AddIn"


  ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\10.0" InstallDir
  ${If} $0 != ""
	  File "/oname=$0..\..\Xml\Schemas\Biml.xsd" "..\bin\DLLs\Biml\Biml.xsd"
  ${Else}
	  #if VS2010 is not installed, then just put the Biml.xsd in what will probably be the VS2010 install directory... that way once they install it, they won't have to reinstall BIDS Helper
	  CreateDirectory "$PROGRAMFILES\Microsoft Visual Studio 10.0\Xml\Schemas"
	  File "/oname=$PROGRAMFILES\Microsoft Visual Studio 10.0\Xml\Schemas\Biml.xsd" "..\bin\DLLs\Biml\Biml.xsd"
  ${EndIf}

  ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\11.0" InstallDir
  ${If} $0 != ""
	  File "/oname=$0..\..\Xml\Schemas\Biml.xsd" "..\bin\DLLs\Biml\Biml.xsd"
  ${Else}
	  #if VS2012 is not installed, then just put the Biml.xsd in what will probably be the VS2012 install directory... that way once they install it, they won't have to reinstall BIDS Helper
	  CreateDirectory "$PROGRAMFILES\Microsoft Visual Studio 11.0\Xml\Schemas"
	  File "/oname=$PROGRAMFILES\Microsoft Visual Studio 11.0\Xml\Schemas\Biml.xsd" "..\bin\DLLs\Biml\Biml.xsd"
  ${EndIf}

SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${VSLOOK_IN_FOLDERS}" "$INSTDIR" ""
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${VSLOOK_IN_FOLDERS2}" "$INSTDIR" ""
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "InstallPath" "$INSTDIR"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"

  ReadRegStr $0 HKCR ".biml" ""
  ${If} $0 == ""
  WriteRegStr HKCR ".biml" "" "BIDSHelper.Biml"
  WriteRegStr HKCR "BIDSHelper.Biml" "" "Business Intelligence Markup Language File"
  WriteRegStr HKCR "BIDSHelper.Biml\DefaultIcon" "" "$INSTDIR\BimlEngine.dll,0"
  System::Call 'shell32.dll::SHChangeNotify(i, i, i, i) v (${SHCNE_ASSOCCHANGED}, ${SHCNF_IDLIST}, 0, 0)'
  ${EndIf}

  #if VS2012 is installed, then we have to also run devenv /setup so that it will pick up the registry changes
  ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\11.0" InstallDir

  ${If} $0 != ""
	  Exec '"$0devenv.exe" /setup'
  ${EndIf}

SectionEnd


Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall

  # check that devenv.exe is not running
  Call un.CloseParentWithUserApproval

# cannot fully uninstall if VS.Net is running and has the dll open.
#  Delete "$INSTDIR\BIDSHelper.dll"
  !insertmacro UnInstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED $INSTDIR\BIDSHelper.dll
  !insertmacro UnInstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED $INSTDIR\Antlr3.Runtime.dll
  !insertmacro UnInstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED $INSTDIR\BimlEngine.dll
  !insertmacro UnInstallLib DLL NOTSHARED NOREBOOT_NOTPROTECTED $INSTDIR\ExpressionEditor.dll

  Delete "$INSTDIR\BIDSHelper2012.Addin"
  DeleteRegValue ${PRODUCT_UNINST_ROOT_KEY} "${VSLOOK_IN_FOLDERS}" "$INSTDIR"
  DeleteRegValue ${PRODUCT_UNINST_ROOT_KEY} "${VSLOOK_IN_FOLDERS2}" "$INSTDIR"
  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey ${PRODUCT_SETTINGS_ROOT_KEY} "${PRODUCT_SETTINGS_KEY}"
  
  ReadRegStr $0 HKCR ".biml" ""
  ${If} $0 == "BIDSHelper.Biml"
  DeleteRegKey HKCR ".biml"
  DeleteRegKey HKCR "BIDSHelper.Biml"
  System::Call 'shell32.dll::SHChangeNotify(i, i, i, i) v (${SHCNE_ASSOCCHANGED}, ${SHCNF_IDLIST}, 0, 0)'
  ${EndIf}
  
  Delete "$INSTDIR\uninst.exe"
  RMDir "$INSTDIR"
  SetAutoClose true
SectionEnd

Function CloseParentWithUserApproval

loop:

  processes::FindProcess "devenv.exe"
  IntCmp $R0 0 done

  MessageBox MB_RETRYCANCEL|MB_ICONSTOP 'Visual Studio.Net must be closed during this installation.$\r$\nClose Visual Studio.Net now, or press $\r$\n"Retry" to automatically close Visual Studio.Net and continue or press $\r$\n"Cancel" to cancel the installation entirely.'  IDCANCEL BailOut

  processes::KillProcess "devenv.exe"
  Sleep 2000
Goto loop

BailOut:
  Abort

done:
FunctionEnd

Function un.CloseParentWithUserApproval

loop:

  processes::FindProcess "devenv.exe"
  IntCmp $R0 0 done

  MessageBox MB_RETRYCANCEL|MB_ICONSTOP 'Visual Studio.Net must be closed during this installation.$\r$\nClose Visual Studio.Net now, or press $\r$\n"Retry" to automatically close Visual Studio.Net and continue or press $\r$\n"Cancel" to cancel the installation entirely.'  IDCANCEL BailOut
  processes::KillProcess "devenv.exe"
  Sleep 2000
Goto loop

BailOut:
  Abort

done:
FunctionEnd
