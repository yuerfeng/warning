# 休息提醒 (WarningApp)

一个基于 .NET 8 WinForms 的定时休息提醒工具，帮助你养成定时休息的好习惯。

## 功能特性

- **定时提醒** — 按设定间隔（默认 45 分钟）弹出全屏休息提醒
- **强制休息** — 休息期间锁定鼠标输入和键盘操作，倒计时结束后自动解除
- **倒计时显示** — 全屏界面显示剩余时间和进度条
- **自定义图片** — 支持 `main.png` 自定义提醒界面背景图
- **托盘运行** — 最小化到系统托盘，不影响日常工作
- **灵活设置** — 可自定义休息间隔和休息时长
- **开机启动** — 安装时可选择开机自动启动
- **设置持久化** — 配置自动保存到 `settings.ini`

## 系统要求

- Windows 10 21H2 及以上
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

## 构建与打包

### 前置依赖

- .NET 8 SDK
- [NSIS](https://nsis.sourceforge.io/Download)（用于生成安装包）

### 构建命令

```powershell
# 编译并打包
.\build.ps1

# 指定版本号
.\build.ps1 -Version "2.0.0"
```

构建产物位于 `bin\Release\net8.0-windows10.0.22000.0\`，安装包为 `WarningApp_Setup_x.x.x.exe`。

## 使用说明

1. 启动后应用自动最小化到系统托盘
2. 右键托盘图标可打开设置或退出
3. 到达设定间隔后弹出全屏休息界面，倒计时结束自动关闭
4. 休息界面中鼠标和键盘被锁定，无法跳过

## 项目结构

```
warning/
├── Program.cs          # 程序入口
├── MainForm.cs         # 主窗体（托盘、定时器、设置读写）
├── SettingForm.cs      # 设置窗体
├── WarningForm.cs      # 休息提醒窗体（全屏锁定）
├── WarningApp.csproj   # 项目配置
├── app.manifest        # 应用清单
├── installer.nsi       # NSIS 安装脚本
├── build.ps1           # 构建打包脚本
├── main.ico            # 应用图标
├── main.png            # 提醒界面背景图
└── license.txt         # MIT 许可证
```

## 许可证

[MIT License](license.txt)
