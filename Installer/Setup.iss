#define public Dependency_NoExampleSetup
#include "CodeDependencies.iss"

#define MyAppName "GestureWheel"
#define MyAppVersion "1.1.0.0"
#define MyAppPublisher "Kodnix"
#define MyAppURL "https://github.com/iodes/GestureWheel"
#define MyAppExeName "GestureWheel.exe"

[Setup]
AppId={{092643FA-40EE-4B00-B3B5-BA794230BB34}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppSupportURL={#MyAppURL}
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
DefaultDirName={autopf}\{#MyAppName}
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
ShowLanguageDialog=auto
OutputBaseFilename={#MyAppName}_{#MyAppVersion}_Setup
CloseApplications=no
WizardStyle=modern

[Messages]
SetupWindowTitle = {#MyAppName} {#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Code]
function InitializeSetup: Boolean;
begin
    Dependency_AddDotNet60Desktop;
    Result := True;
end;

procedure TaskKill(FileName: String);
var
    ResultCode: Integer;
begin
    Exec('taskkill.exe', '/f /im ' + '"' + FileName + '"', '', SW_HIDE,
    ewWaitUntilTerminated, ResultCode);
end;

function InitializeUninstall(): Boolean;
    var ErrorCode: Integer;
begin
    TaskKill('{#MyAppExeName}')
    result := True;
end;

[Files]
Source: "Utilities\*"; Flags: dontcopy noencryption
Source: "..\GestureWheel\bin\Release\net6.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs; BeforeInstall: TaskKill('{#MyAppExeName}')

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Parameters: "/Activate"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Parameters: "/Activate"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Parameters: "/Activate"; Flags: nowait postinstall skipifsilent
