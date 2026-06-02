; Sim Racing Companion — Inno Setup Script
; Version: 3.0.0

#define AppName "Sim Racing Companion"
#define AppVersion "3.0.0"
#define AppPublisher "Sim Racing Companion"
#define AppExeName "SimRacingCompanion.exe"
#define AppGUID "{8A9B3C4D-5E6F-7890-ABCD-EF1234567890}"

[Setup]
AppId={{{#AppGUID}}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\SimRacingCompanion
DefaultGroupName={#AppName}
OutputDir=..\dist
OutputBaseFilename=SimRacingCompanion-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#AppExeName}
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
MinVersion=10.0.17763
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "ptbr"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "autostart"; Description: "{cm:AutoStartTask}"; GroupDescription: "{cm:StartupGroup}"; Flags: checkedonce

[Files]
; Main executable (single-file published)
Source: "..\publish\win-x64\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; String resources (copied next to exe)
Source: "..\resources\strings\pt-BR.json"; DestDir: "{app}\resources\strings"; Flags: ignoreversion
Source: "..\resources\strings\en.json"; DestDir: "{app}\resources\strings"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Desinstalar {#AppName}"; Filename: "{uninstallexe}"

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; \
  ValueType: string; ValueName: "SimRacingCompanion"; \
  ValueData: """{app}\{#AppExeName}"""; \
  Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Iniciar {#AppName}"; \
  Flags: nowait postinstall skipifsilent

[CustomMessages]
ptbr.AutoStartTask=Iniciar automaticamente com o Windows
ptbr.StartupGroup=Inicialização
english.AutoStartTask=Start automatically with Windows
english.StartupGroup=Startup
ptbr.WelcomeTitle=Bem-vindo ao Sim Racing Companion
ptbr.WelcomeDesc=Este assistente irá instalar o Sim Racing Companion no seu computador.
english.WelcomeTitle=Welcome to Sim Racing Companion
english.WelcomeDesc=This wizard will install Sim Racing Companion on your computer.

[Code]
var
  ModePage: TInputOptionWizardPage;
  AcPathPage: TInputDirWizardPage;
  CmPathPage: TInputDirWizardPage;
  SelectedMode: string;
  SelectedLanguage: string;

function DetectSteamPath(): string;
var
  SteamPath: string;
begin
  Result := '';
  if RegQueryStringValue(HKCU, 'SOFTWARE\Valve\Steam', 'SteamPath', SteamPath) then
  begin
    SteamPath := SteamPath + '\steamapps\common\assettocorsa';
    if DirExists(SteamPath) then
      Result := SteamPath;
  end;
  if Result = '' then
  begin
    if DirExists('C:\Program Files (x86)\Steam\steamapps\common\assettocorsa') then
      Result := 'C:\Program Files (x86)\Steam\steamapps\common\assettocorsa'
    else if DirExists('C:\Program Files\Steam\steamapps\common\assettocorsa') then
      Result := 'C:\Program Files\Steam\steamapps\common\assettocorsa';
  end;
end;

function DetectCmSessionsPath(): string;
var
  Path: string;
begin
  Path := ExpandConstant('{localappdata}') + '\AcTools Content Manager\Progress\Sessions';
  if DirExists(Path) then
    Result := Path
  else
    Result := '';
end;

procedure InitializeWizard();
var
  AcDefault, CmDefault: string;
begin
  // Detect paths
  AcDefault := DetectSteamPath();
  CmDefault := DetectCmSessionsPath();

  // Mode selection page
  ModePage := CreateInputOptionPage(
    wpSelectDir,
    'Modo de operação', 'Como este computador será usado?',
    'Selecione o modo de operação:', False, False);
  ModePage.Add('Fonte de dados — este PC tem o Assetto Corsa instalado (envia dados para a plataforma)');
  ModePage.Add('Somente visualização — este PC não tem o AC (apenas recebe dados da plataforma)');
  ModePage.Values[0] := True;

  // AC path page
  AcPathPage := CreateInputDirPage(
    ModePage.ID,
    'Pasta do Assetto Corsa', 'Onde o Assetto Corsa está instalado?',
    'O instalador detectou a pasta abaixo. Confirme ou selecione manualmente:',
    False, '');
  AcPathPage.Add('Pasta do Assetto Corsa:');
  AcPathPage.Values[0] := AcDefault;

  // CM Sessions path page
  CmPathPage := CreateInputDirPage(
    AcPathPage.ID,
    'Pasta de Sessões', 'Onde o Content Manager salva as sessões?',
    'Pasta de sessões do Content Manager (deixe em branco para usar o padrão):',
    False, '');
  CmPathPage.Add('Pasta de sessões:');
  CmPathPage.Values[0] := CmDefault;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  // Skip AC/CM path pages in viewer mode
  if (PageID = AcPathPage.ID) or (PageID = CmPathPage.ID) then
    Result := ModePage.Values[1];  // viewer mode selected
end;

function WriteSettingsJson(): Boolean;
var
  SettingsDir, SettingsFile, Json: string;
  Language, Mode, AcPath, CmPath: string;
begin
  SettingsDir := ExpandConstant('{userappdata}') + '\SimRacingCompanion';
  SettingsFile := SettingsDir + '\settings.json';

  if not ForceDirectories(SettingsDir) then
  begin
    Result := False;
    Exit;
  end;

  // Determine language
  if ActiveLanguage = 'ptbr' then
    Language := 'pt-BR'
  else
    Language := 'en';

  // Determine mode
  if ModePage.Values[0] then
    Mode := 'source'
  else
    Mode := 'viewer';

  AcPath := AcPathPage.Values[0];
  CmPath := CmPathPage.Values[0];

  // Build JSON manually (Inno Setup has no JSON library)
  Json := '{' + #13#10;
  Json := Json + '  "SupabaseUrl": "https://nnhbowhfqjucedjnsvtp.supabase.co",' + #13#10;
  Json := Json + '  "SupabaseAnonKey": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5uaGJvd2hmcWp1Y2Vkam5zdnRwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzkyMzEwMjgsImV4cCI6MjA5NDgwNzAyOH0.83_32291NL4y4p4J8FjgpGTy2_XSO03PJ9edMgs8zy8",' + #13#10;
  Json := Json + '  "WebAppUrl": "https://sim-racing-companion.vercel.app",' + #13#10;
  Json := Json + '  "UserToken": "",' + #13#10;
  Json := Json + '  "RefreshToken": "",' + #13#10;
  Json := Json + '  "UserEmail": "",' + #13#10;
  Json := Json + '  "SyncIntervalMinutes": 5,' + #13#10;
  Json := Json + '  "AutoStart": ' + BoolToStr(IsTaskSelected('autostart'), True) + ',' + #13#10;
  Json := Json + '  "Mode": "' + Mode + '",' + #13#10;
  Json := Json + '  "Language": "' + Language + '",' + #13#10;
  Json := Json + '  "AcPath": "' + StringReplace(AcPath, '\', '\\', [rfReplaceAll]) + '",' + #13#10;
  Json := Json + '  "CmSessionsPath": "' + StringReplace(CmPath, '\', '\\', [rfReplaceAll]) + '",' + #13#10;
  Json := Json + '  "PersonalBestPath": "",' + #13#10;
  Json := Json + '  "WebSyncPollSeconds": 30' + #13#10;
  Json := Json + '}';

  Result := SaveStringToFile(SettingsFile, Json, False);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    WriteSettingsJson();
end;
