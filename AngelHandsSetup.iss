; Inno Setup Script for Angel Hands

[Setup]
; Basic setup information
AppName=Angel Hands
AppVersion=1.0
DefaultDirName={pf}\Angel Hands
DefaultGroupName=Angel Hands
OutputBaseFilename=AngelHandsSetup
SetupIconFile=UnityGame\AngelHands.ico
UninstallDisplayIcon={app}\AngelHands.exe
Compression=lzma
SolidCompression=yes

; Specifies the installer window style
WizardImageFile=compiler:WizModernImage.bmp
WizardSmallImageFile=compiler:WizModernSmallImage.bmp

[Files]
; Copy all game files to the installation directory
Source: "UnityGame\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Create a desktop icon for the game
Name: "{userdesktop}\Angel Hands"; Filename: "{app}\AngelHands.exe"; IconFilename: "{app}\AngelHands.ico"
; Create a Start Menu icon for the game
Name: "{group}\Angel Hands"; Filename: "{app}\AngelHands.exe"; IconFilename: "{app}\AngelHands.ico"

[Run]
; No application to run after installation

[UninstallDelete]
; Optionally delete other non-game files on uninstall
Type: filesandordirs; Name: "{app}"
