# Scripts/generate-openapi.ps1
# OpenAPI JSON dosyalarını API assembly'lerinden otomatik olarak üretir.
# CI/CD pipeline'larında "drift check" için veya yerel geliştirmede kullanılabilir.

param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

# Scriptin bulunduğu dizini baz alarak root path'i belirle
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$rootPath = (Get-Item (Join-Path $scriptDir "..")).FullName

$docsPath = Join-Path $rootPath "Docs"
$binPath = "bin/$Configuration/net10.0"

Write-Host "--- OpenAPI JSON Üretimi Başlatılıyor ---" -ForegroundColor Cyan
Write-Host "Root Path: $rootPath"
Write-Host "Configuration: $Configuration"

# 1. Dotnet tool'larını restore et
Write-Host "Dotnet tools restore ediliyor..."
dotnet tool restore

# 2. ExpenseService.Api
Write-Host "ExpenseService.Api build ediliyor ($Configuration)..."
dotnet build (Join-Path $rootPath "src/Services/ExpenseService/ExpenseService.Api/ExpenseService.Api.csproj") -c $Configuration

Write-Host "openapi-expense.json üretiliyor..."
$expenseDll = Join-Path $rootPath "src/Services/ExpenseService/ExpenseService.Api/$binPath/ExpenseService.Api.dll"
dotnet tool run swagger tofile --output (Join-Path $docsPath "openapi-expense.json") $expenseDll v1

# 3. NotificationService.Api
Write-Host "NotificationService.Api build ediliyor ($Configuration)..."
dotnet build (Join-Path $rootPath "src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj") -c $Configuration

Write-Host "openapi-notification.json üretiliyor..."
$notificationDll = Join-Path $rootPath "src/Services/NotificationService/NotificationService.Api/$binPath/NotificationService.Api.dll"
dotnet tool run swagger tofile --output (Join-Path $docsPath "openapi-notification.json") $notificationDll v1

Write-Host "--- OpenAPI JSON Üretimi Başarıyla Tamamlandı ---" -ForegroundColor Green
Write-Host "Dosyalar: $docsPath"
