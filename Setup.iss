#define MyAppName "Bing Wallpaper"
#define MyAppVersion "1.2.2"
#define MyAppPublisher "Jesse Stricker"
#define MyAppURL "https://github.com/jessestricker/BingWallpaper"
#define MyAppExeName "Bing Wallpaper.exe"

[Setup]
AppId={{F9DA6E85-88D6-4296-A3CD-60D63542E776}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=BingWallpaperSetup
OutputDir=SetupOutput
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Files]
Source: "bin\Release\Bing Wallpaper.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\de\*"; DestDir: "{app}\de"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

