param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ProjectDir "WarningApp.csproj"
$PublishDir = Join-Path $ProjectDir "bin\$Configuration\net8.0-windows10.0.22000.0"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  休息提醒 - 编译打包脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/4] 清理旧的发布文件..." -ForegroundColor Yellow
if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
    Write-Host "  已清理 $PublishDir" -ForegroundColor Gray
}

Write-Host ""
Write-Host "[2/4] 编译项目 ($Configuration)..." -ForegroundColor Yellow
dotnet build $ProjectFile -c $Configuration /p:Version=$Version
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败！" -ForegroundColor Red
    exit 1
}
Write-Host "  编译成功" -ForegroundColor Green

Write-Host ""
Write-Host "[3/4] 复制打包资源..." -ForegroundColor Yellow
$Resources = @("installer.nsi", "main.ico", "main.png")
foreach ($res in $Resources) {
    $src = Join-Path $ProjectDir $res
    $dst = Join-Path $PublishDir $res
    if (Test-Path $src) {
        Copy-Item $src $dst -Force
        Write-Host "  复制 $res" -ForegroundColor Gray
    } else {
        Write-Host "  警告: 未找到 $res" -ForegroundColor DarkYellow
    }
}

Write-Host ""
Write-Host "[4/4] 生成安装包 (NSIS)..." -ForegroundColor Yellow

$nsisPaths = @(
    "C:\Program Files (x86)\NSIS\makensis.exe",
    "C:\Program Files\NSIS\makensis.exe",
    (Get-Command makensis -ErrorAction SilentlyContinue).Source
)

$makensis = $null
foreach ($p in $nsisPaths) {
    if ($p -and (Test-Path $p)) {
        $makensis = $p
        break
    }
}

if (-not $makensis) {
    Write-Host "未找到 NSIS (makensis.exe)！" -ForegroundColor Red
    Write-Host "请安装 NSIS: https://nsis.sourceforge.io/Download" -ForegroundColor Red
    Write-Host ""
    Write-Host "编译产物已输出到: $PublishDir" -ForegroundColor Green
    Write-Host "请手动将 installer.nsi 中的 PRODUCT_DIR 指向该目录后运行 makensis" -ForegroundColor DarkYellow
    exit 1
}

$nsiFile = Join-Path $PublishDir "installer.nsi"

$tempNsi = Join-Path $PublishDir "installer_build.nsi"
$content = [System.IO.File]::ReadAllText($nsiFile, [System.Text.UTF8Encoding]::new($true))
$content = $content.TrimStart([char]0xFEFF)
$content = $content -replace '!define PRODUCT_DIR "\."', "!define PRODUCT_DIR ""$($PublishDir -replace '\\','\\')"""
$utf8Bom = [System.Text.UTF8Encoding]::new($true)
[System.IO.File]::WriteAllText($tempNsi, $content, $utf8Bom)

& $makensis $tempNsi
if ($LASTEXITCODE -ne 0) {
    Write-Host "NSIS 打包失败！" -ForegroundColor Red
    Remove-Item $tempNsi -Force -ErrorAction SilentlyContinue
    exit 1
}

Remove-Item $tempNsi -Force -ErrorAction SilentlyContinue

$setupExe = Join-Path $PublishDir "WarningApp_Setup_$Version.exe"
if (Test-Path $setupExe) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  打包完成！" -ForegroundColor Green
    Write-Host "  安装包: $setupExe" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
} else {
    Write-Host "安装包文件未找到，请检查 NSIS 输出" -ForegroundColor DarkYellow
}
