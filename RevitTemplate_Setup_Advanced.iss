#define MyAppName "Revit Template"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Petr Mitev"
#define MyAppURL "https://github.com/mitevpi"
#define MyAppExeName "RevitTemplate.dll"
#define SignalRWorkerExeName "SignalRWorker.exe"

[Setup]
AppId={{80671941-95DE-4707-9C40-758E89CBD063}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=License.txt
OutputDir=Output
OutputBaseFilename=RevitTemplate_Setup
SetupIconFile=Resources\revitIcon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
MinVersion=10.0.17763
DisableProgramGroupPage=yes
DisableDirPage=no
UsePreviousAppDir=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenuicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; SignalR Worker files
Source: "SignalRWorker\bin\Release\net8.0\*"; DestDir: "{app}\SignalRWorker"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "SignalRWorker\bin\Release\net8.0\SignalRWorker.exe"; DestDir: "{app}\SignalRWorker"; Flags: ignoreversion

; Revit Template files
Source: "Revit Template\bin\Release\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Revit Template\RevitTemplate.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024"; Flags: ignoreversion

; Resources
Source: "Revit Template\Resources\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024\Resources"; Flags: ignoreversion recursesubdirs createallsubdirs

; Configuration file
Source: "config.json"; DestDir: "{userappdata}\RevitTemplate"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\SignalRWorker\{#SignalRWorkerExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\SignalRWorker\{#SignalRWorkerExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\SignalRWorker\{#SignalRWorkerExeName}"; Tasks: startmenuicon

[Registry]
; Register the add-in with Revit
Root: HKCU; Subkey: "Software\Autodesk\Revit\Autodesk Revit 2024\Addins\{#MyAppName}"; ValueType: string; ValueName: "Assembly"; ValueData: "{userappdata}\Autodesk\Revit\Addins\2024\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Autodesk\Revit\Autodesk Revit 2024\Addins\{#MyAppName}"; ValueType: string; ValueName: "FullClassName"; ValueData: "RevitTemplate.RevitApp"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Autodesk\Revit\Autodesk Revit 2024\Addins\{#MyAppName}"; ValueType: string; ValueName: "AddInId"; ValueData: "604b1052-f742-4127-8576-c821d1193102"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Autodesk\Revit\Autodesk Revit 2024\Addins\{#MyAppName}"; ValueType: string; ValueName: "VendorId"; ValueData: "Petr Mitev"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Autodesk\Revit\Autodesk Revit 2024\Addins\{#MyAppName}"; ValueType: string; ValueName: "VendorDescription"; ValueData: "https://github.com/mitevpi"; Flags: uninsdeletekey

; Store SignalR Worker path
Root: HKCU; Subkey: "Software\{#MyAppName}"; ValueType: string; ValueName: "SignalRWorkerPath"; ValueData: "{app}\SignalRWorker\{#SignalRWorkerExeName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey

[Run]
; Install .NET 8.0 Runtime if not present
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""& {[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'; & .\dotnet-install.ps1 -Channel 8.0 -Runtime dotnet -Architecture x64}"""; Flags: runascurrentuser shellexec; Check: NotDotNet8Installed; StatusMsg: "Instalando .NET 8.0 Runtime..."

; Start SignalR Worker service
Filename: "{app}\SignalRWorker\{#SignalRWorkerExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  RevitInstalled: Boolean;
  DotNet8Installed: Boolean;

function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Check if Revit 2024 is installed
  RevitInstalled := RegKeyExists(HKLM, 'SOFTWARE\Autodesk\Revit\Autodesk Revit 2024') or
                    RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\Autodesk\Revit\Autodesk Revit 2024');
                    
  if not RevitInstalled then
  begin
    MsgBox('Revit 2024 não foi detectado no sistema.' + #13#10 + 
           'Este add-in requer o Autodesk Revit 2024 para funcionar.', 
           mbInformation, MB_OK);
  end;
  
  // Check if .NET 8.0 Runtime is installed
  DotNet8Installed := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{2E5C0757-8A1E-4B8A-9F0A-8E1E9F9F9F9F}') or
                      RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{2E5C0757-8A1E-4B8A-9F0A-8E1E9F9F9F9F}');
end;

function NotDotNet8Installed: Boolean;
begin
  Result := not DotNet8Installed;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  
  if CurPageID = wpReady then
  begin
    // Create configuration file with SignalR Worker path
    SaveStringToFile(ExpandConstant('{userappdata}\RevitTemplate\config.json'), 
      '{"SignalRWorkerPath": "' + ExpandConstant('{app}\SignalRWorker\{#SignalRWorkerExeName}') + '",' +
      '"HubUrl": "https://localhost:6102/revitHub",' +
      '"LogLevel": "Info"}', False);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    // Set proper permissions for the add-in directory
    Exec('icacls', ExpandConstant('"{userappdata}\Autodesk\Revit\Addins\2024"') + ' /grant Users:F /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    
    // Set proper permissions for the SignalR Worker directory
    Exec('icacls', ExpandConstant('"{app}\SignalRWorker"') + ' /grant Users:F /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    
    // Create log directory
    CreateDir(ExpandConstant('{userappdata}\RevitTemplate\Logs'));
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Clean up configuration files
    DeleteFile(ExpandConstant('{userappdata}\RevitTemplate\config.json'));
    RemoveDir(ExpandConstant('{userappdata}\RevitTemplate'));
  end;
end;

[UninstallDelete]
Type: files; Name: "{userappdata}\RevitTemplate\config.json"
Type: dirifempty; Name: "{userappdata}\RevitTemplate"
Type: dirifempty; Name: "{userappdata}\Autodesk\Revit\Addins\2024"
