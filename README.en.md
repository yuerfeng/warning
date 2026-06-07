# Break Reminder (WarningApp)

A .NET 8 WinForms-based timed break reminder tool that helps you build a healthy habit of taking regular breaks.

## Features

- **Timed Reminders** — Full-screen break reminder at configurable intervals (default: 45 minutes)
- **Enforced Breaks** — Locks mouse and keyboard input during break; auto-releases when countdown ends
- **Countdown Display** — Full-screen interface showing remaining time and progress bar
- **Custom Image** — Supports `main.png` as a custom background for the reminder screen
- **System Tray** — Minimizes to system tray, staying out of your way
- **Flexible Settings** — Customizable break interval and break duration
- **Auto Start** — Option to launch at Windows startup during installation
- **Persistent Settings** — Configuration saved automatically to `settings.ini`

## System Requirements

- Windows 10 21H2 or later
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build & Package

### Prerequisites

- .NET 8 SDK
- [NSIS](https://nsis.sourceforge.io/Download) (for generating the installer)

### Build Commands

```powershell
# Build and package
.\build.ps1

# Specify version
.\build.ps1 -Version "2.0.0"
```

Build output is located in `bin\Release\net8.0-windows10.0.22000.0\`, and the installer is `WarningApp_Setup_x.x.x.exe`.

## Usage

1. After launch, the app minimizes to the system tray automatically
2. Right-click the tray icon to open settings or exit
3. When the interval is reached, a full-screen break reminder appears and closes automatically when the countdown ends
4. Mouse and keyboard are locked during the break — you cannot skip it

## Project Structure

```
warning/
├── Program.cs          # Entry point
├── MainForm.cs         # Main form (tray, timer, settings I/O)
├── SettingForm.cs      # Settings form
├── WarningForm.cs      # Break reminder form (full-screen lock)
├── WarningApp.csproj   # Project configuration
├── app.manifest        # Application manifest
├── installer.nsi       # NSIS installer script
├── build.ps1           # Build & package script
├── main.ico            # Application icon
├── main.png            # Reminder screen background
└── license.txt         # MIT License
```

## License

[MIT License](license.txt)
