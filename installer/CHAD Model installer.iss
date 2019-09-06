; Extension infomation
#define ExtensionName "CHAD Model"
#define AppVersion "1.24"
#define AppPublisher "Garry Sotnik"

; Build directory
#define BuildDir "..\CHAD Model\DesktopApplication\bin\Release"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{FD55A4F8-545D-438C-A2AF-0B2EF64222FF}
AppName={#ExtensionName}
AppVersion={#AppVersion}
; Name in "Programs and Features"
AppVerName={#ExtensionName}
AppPublisher={#AppPublisher}
;AppPublisherURL={#AppURL}
;AppSupportURL={#AppURL}
;AppUpdatesURL={#AppURL}
DefaultDirName=D:\{#ExtensionName}
DisableDirPage=no
DefaultGroupName={#ExtensionName}
DisableProgramGroupPage=yes
LicenseFile=THE SOSIEL LICENSE AGREEMENT.rtf
OutputDir={#SourcePath}
OutputBaseFilename={#ExtensionName} latest-setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: {app}; Permissions: users-modify

[Files]
Source: {#BuildDir}\*; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs     

;[Run]



;[UninstallRun]



