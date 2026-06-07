!define PRODUCT_NAME "休息提醒"
!define PRODUCT_NAME_EN "WarningApp"
!define PRODUCT_VERSION "1.0.0"
!define PRODUCT_PUBLISHER "WarningApp"
!define PRODUCT_EXE "WarningApp.exe"
!define PRODUCT_ICON "main.ico"
!define PRODUCT_DIR "."

!include "MUI2.nsh"
!include "FileFunc.nsh"

Name "${PRODUCT_NAME}"
OutFile "${PRODUCT_NAME_EN}_Setup_${PRODUCT_VERSION}.exe"
InstallDir "$PROGRAMFILES\${PRODUCT_NAME_EN}"
InstallDirRegKey HKLM "Software\${PRODUCT_NAME_EN}" "InstallDir"
RequestExecutionLevel admin
CRCCheck on

!define MUI_ICON "${PRODUCT_DIR}\${PRODUCT_ICON}"
!define MUI_UNICON "${PRODUCT_DIR}\${PRODUCT_ICON}"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN "$INSTDIR\${PRODUCT_EXE}"
!define MUI_FINISHPAGE_RUN_TEXT "运行 ${PRODUCT_NAME}"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "SimpChinese"

Section "!${PRODUCT_NAME}" SEC_MAIN
    SetOutPath "$INSTDIR"
    SetOverwrite on

    File "${PRODUCT_DIR}\WarningApp.exe"
    File "${PRODUCT_DIR}\WarningApp.dll"
    File "${PRODUCT_DIR}\WarningApp.deps.json"
    File "${PRODUCT_DIR}\WarningApp.runtimeconfig.json"
    File "${PRODUCT_DIR}\Microsoft.Windows.SDK.NET.dll"
    File "${PRODUCT_DIR}\WinRT.Runtime.dll"
    File "${PRODUCT_DIR}\main.ico"
    File "${PRODUCT_DIR}\main.png"

    WriteRegStr HKLM "Software\${PRODUCT_NAME_EN}" "InstallDir" "$INSTDIR"
    WriteUninstaller "$INSTDIR\uninstall.exe"

    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME_EN}" "DisplayName" "${PRODUCT_NAME}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME_EN}" "UninstallString" "$INSTDIR\uninstall.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME_EN}" "DisplayIcon" "$INSTDIR\${PRODUCT_ICON}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME_EN}" "DisplayVersion" "${PRODUCT_VERSION}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME_EN}" "Publisher" "${PRODUCT_PUBLISHER}"
    ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
    IntFmt $0 "0x%08X" $0
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME_EN}" "EstimatedSize" "$0"

    CreateShortCut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\${PRODUCT_EXE}" "" "$INSTDIR\${PRODUCT_ICON}"
    CreateDirectory "$SMPROGRAMS\${PRODUCT_NAME}"
    CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME}.lnk" "$INSTDIR\${PRODUCT_EXE}" "" "$INSTDIR\${PRODUCT_ICON}"
    CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\卸载.lnk" "$INSTDIR\uninstall.exe"
SectionEnd

Section "开机启动" SEC_AUTOSTART
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "${PRODUCT_NAME_EN}" "$\"$INSTDIR\${PRODUCT_EXE}$\""
SectionEnd

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SEC_MAIN} "安装 ${PRODUCT_NAME} 主程序"
    !insertmacro MUI_DESCRIPTION_TEXT ${SEC_AUTOSTART} "开机时自动运行 ${PRODUCT_NAME}"
!insertmacro MUI_FUNCTION_DESCRIPTION_END

Section "Uninstall"
    Delete "$DESKTOP\${PRODUCT_NAME}.lnk"
    RMDir /r "$SMPROGRAMS\${PRODUCT_NAME}"

    Delete "$INSTDIR\WarningApp.exe"
    Delete "$INSTDIR\WarningApp.dll"
    Delete "$INSTDIR\WarningApp.deps.json"
    Delete "$INSTDIR\WarningApp.runtimeconfig.json"
    Delete "$INSTDIR\Microsoft.Windows.SDK.NET.dll"
    Delete "$INSTDIR\WinRT.Runtime.dll"
    Delete "$INSTDIR\main.ico"
    Delete "$INSTDIR\main.png"
    Delete "$INSTDIR\uninstall.exe"

    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME_EN}"
    DeleteRegKey HKLM "Software\${PRODUCT_NAME_EN}"
    DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "${PRODUCT_NAME_EN}"

    RMDir "$INSTDIR"
SectionEnd
