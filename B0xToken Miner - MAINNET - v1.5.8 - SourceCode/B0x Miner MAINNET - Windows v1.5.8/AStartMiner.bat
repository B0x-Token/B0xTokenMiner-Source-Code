@echo off
setlocal
chcp 65001
set "REPAIR_MODE=0"
:startstart
echo ============================================
echo Checking for ASP.NET Core 6.0 Runtime...
echo ============================================
:: --- Check if dotnet command exists ---
where dotnet >nul 2>&1
if %errorlevel% NEQ 0 (
    echo dotnet command not found. Installing ASP.NET Core 6.0 Runtime...
    goto installRuntime
)
:: --- Check for ASP.NET Core 6.0 Runtime ---
dotnet --list-runtimes | findstr "Microsoft.AspNetCore.App 6." >nul 2>&1
if %errorlevel% EQU 0 (
    echo ASP.NET Core 6.0 Runtime found.
    goto continueProgram
)
:installRuntime
echo ASP.NET Core 6.0 Runtime not found. Installing...
echo This will download and install ASP.NET Core 6.0 Runtime automatically.
echo Please wait...
:: --- Download ASP.NET Core Runtime ---
set "URL=https://builds.dotnet.microsoft.com/dotnet/Runtime/6.0.36/dotnet-runtime-6.0.36-win-x64.exe"
set "OUT=%TEMP%\aspnetcore-runtime-6.0.36-win-x64.exe"
echo Downloading %URL% to %OUT% ...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest -Uri '%URL%' -OutFile '%OUT%' -UseBasicParsing"
if errorlevel 1 (
    echo Failed to download the installer.
    pause
    exit /b 1
)
:: --- Run installer interactively ---
echo Starting installer (you should see the setup wizard)...
if "%REPAIR_MODE%"=="1" (
    echo CLICK REPAIR in installer
)
powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%OUT%' -Verb RunAs -Wait"
:: --- Check exit code ---
if %errorlevel%==0 (
    echo Installation completed successfully.
) else (
    echo Installer exited with code %errorlevel%.
    echo Please complete the installation wizard.
)
:: --- Wait for PATH to update ---
echo.
echo Waiting for system PATH to update...
timeout /t 5 /nobreak >nul
:: --- Verify installation ---
echo Verifying ASP.NET Core 6.0 installation...
:: Refresh environment variables
call :RefreshEnv
where dotnet >nul 2>&1
if %errorlevel% NEQ 0 (
    echo WARNING: dotnet command still not found after installation.
    echo The installer may require a system restart.
    echo.
    echo Please restart your computer and run this script again.
    pause
    exit /b 1
)
dotnet --list-runtimes | findstr "Microsoft.AspNetCore.App 6." >nul 2>&1
if errorlevel 1 (
    echo ASP.NET Core 6.0 installation verification failed.
    echo Please ensure ASP.NET Core 6.0 Runtime is properly installed.
    pause
    exit /b 1
)
echo ASP.NET Core 6.0 Runtime successfully installed!
:continueProgram
echo.
echo Sleeping for 3 seconds before launch...
timeout /t 3 /nobreak >nul
:startProgram
echo.
echo Starting B0xToken.exe...
B0xToken.exe
:: Check the actual program exit code
if %errorlevel% EQU 22 (
    echo Exit code 22 detected - restarting...
    timeout /t 5
    goto startProgram
)
pause
timeout /t 2
pause
timeout /t 3
pause
endlocal
exit /b 0
:: Function to refresh environment variables
:RefreshEnv
for /f "tokens=2*" %%a in ('reg query "HKLM\System\CurrentControlSet\Control\Session Manager\Environment" /v Path ^| findstr /i "Path"') do set "SysPath=%%b"
for /f "tokens=2*" %%a in ('reg query "HKCU\Environment" /v Path ^| findstr /i "Path"') do set "UsrPath=%%b"
set "PATH=%SysPath%;%UsrPath%"
echo Sometimes it takes 1 Install and 1 Repair for dot net to work correctly. 
echo IF Installer runs again click REPAIR.
:: --- Check if we're in repair mode and installation already failed ---
if "%REPAIR_MODE%"=="1" (
    echo.
    echo ============================================
    echo AUTOMATIC INSTALLATION HAS FAILED
    echo ============================================
    echo.
    echo Please manually download and install the .NET Runtime:
    echo.
    echo Download URL:
    echo https://builds.dotnet.microsoft.com/dotnet/Runtime/6.0.36/dotnet-runtime-6.0.36-win-x64.exe
    echo.
    echo Please manually install the dotnet
    echo ============================================
    pause
    pause
    pause
    exit /b 1
)

set "REPAIR_MODE=1"

goto startstart
exit /b