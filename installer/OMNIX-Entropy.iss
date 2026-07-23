#ifndef MyAppVersion
  #error MyAppVersion is required.
#endif
#ifndef SourcePackage
  #error SourcePackage is required.
#endif
#ifndef OutputDirectory
  #error OutputDirectory is required.
#endif

[Setup]
AppId={{3AA3FE2C-2E58-46F0-B0EE-BC3D96A0CEB8}
AppName=OMNIX-Entropy
AppVersion={#MyAppVersion}
AppPublisher=plnoble
AppPublisherURL=https://github.com/plnoble/OMNIX-Entropy
AppSupportURL=https://github.com/plnoble/OMNIX-Entropy/issues
AppUpdatesURL=https://github.com/plnoble/OMNIX-Entropy/releases
DefaultDirName=D:\Software\OMNIX-Entropy\Install
DefaultGroupName=OMNIX-Entropy
DisableDirPage=no
DisableProgramGroupPage=yes
AllowNoIcons=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
OutputDir={#OutputDirectory}
OutputBaseFilename=OMNIX-Entropy-{#MyAppVersion}-win-x64-setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
SetupLogging=yes
UsePreviousAppDir=yes
Uninstallable=yes
UninstallDisplayIcon={app}\Css.App.exe
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName=OMNIX-Entropy
VersionInfoProductVersion={#MyAppVersion}
SignTool=omnix
SignedUninstaller=yes
ChangesAssociations=no
ChangesEnvironment=no

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourcePackage}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\OMNIX-Entropy"; Filename: "{app}\Css.App.exe"
Name: "{autodesktop}\OMNIX-Entropy"; Filename: "{app}\Css.App.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Css.App.exe"; Description: "{cm:LaunchProgram,OMNIX-Entropy}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
  Result := not WizardSilent;
  if not Result then
    MsgBox('OMNIX-Entropy requires a visible installation confirmation.', mbError, MB_OK);
end;
