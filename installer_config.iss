; Script de instalação para Revit Template Addin
; Gerado automaticamente - não editar manualmente

[Setup]
AppName=Revit Template Addin
AppVersion=1.0.0
AppPublisher=Info W Software
AppPublisherURL=https://github.com/mitevpi
DefaultDirName={userappdata}\Autodesk\Revit\Addins\2024
DefaultGroupName=Revit Template
OutputDir=installer_output
OutputBaseFilename=RevitTemplate_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"

[Files]
; Arquivos principais do addin
Source: "Revit Template\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Revit Template\RevitTemplate.addin"; DestDir: "{app}"; Flags: ignoreversion

; SignalRWorker
Source: "Revit Template\bin\Release\Worker\SignalRWorker.exe"; DestDir: "{app}\Worker"; Flags: ignoreversion

; Documentação
Source: "Revit Template\License.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion; DestName: "README.md"
Source: "PUBLISH_README.md"; DestDir: "{app}"; Flags: ignoreversion; DestName: "PUBLISH_README.md"

[Icons]
Name: "{group}\Revit Template Addin"; Filename: "{app}\RevitTemplate.addin"

[Run]
Filename: "{app}\PUBLISH_README.md"; Description: "Ver documentação do addin"; Flags: postinstall shellexec

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Mostrar mensagem informativa
         MsgBox('Instalação concluída!' + #13#10 + #13#10 + 
            'Para que o addin seja carregado:' + #13#10 +
            '1. Feche o Revit 2024 se estiver aberto' + #13#10 +
            '2. Reinicie o Revit 2024' + #13#10 +
            '3. O addin estará disponível na aba "OLIMPO" do ribbon', 
            mbInformation, MB_OK);
  end;
end;
