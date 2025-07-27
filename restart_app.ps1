# PowerShell script to restart the application
Write-Host "🔄 Restarting GymManagement.Web application..." -ForegroundColor Yellow

# Kill existing processes
Write-Host "🛑 Killing existing processes..." -ForegroundColor Red
Get-Process -Name "GymManagement.Web" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*GymManagement*" } | Stop-Process -Force

# Wait a moment
Start-Sleep -Seconds 2

# Change to project directory
Set-Location "D:\DATN\HANG_REMOTE\qlpg\GymManagement.Web"

# Clean and build
Write-Host "🧹 Cleaning project..." -ForegroundColor Blue
dotnet clean

Write-Host "🔨 Building project..." -ForegroundColor Blue
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful! Starting application..." -ForegroundColor Green

    # Start the application
    Write-Host "🚀 Starting application on http://localhost:5003..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --urls http://localhost:5003"

    # Wait a moment and open browser
    Start-Sleep -Seconds 5
    Write-Host "🌐 Opening browser..." -ForegroundColor Cyan
    Start-Process "http://localhost:5003/GoiTap"

    Write-Host "✅ Application started successfully!" -ForegroundColor Green
    Write-Host "📝 Test steps:" -ForegroundColor Yellow
    Write-Host "1. Login as admin (admin/Admin@123)" -ForegroundColor White
    Write-Host "2. Go to GoiTap management" -ForegroundColor White
    Write-Host "3. Click 'Thêm gói tập mới'" -ForegroundColor White
    Write-Host "4. Fill form and submit" -ForegroundColor White
    Write-Host "5. Check if HTTP 400 error is resolved" -ForegroundColor White
}
else {
    Write-Host "❌ Build failed! Check the errors above." -ForegroundColor Red
}

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
