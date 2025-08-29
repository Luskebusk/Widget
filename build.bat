@echo off
echo Building Mobit System Info Widget...
echo.

REM Clean previous builds
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj" rmdir /s /q "obj"

REM Build and publish as single file
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build completed successfully!
    echo.
    echo Output file: bin\Release\net6.0-windows\win-x64\publish\MobitSystemInfoWidget.exe
    echo.
    echo File size:
    for %%I in ("bin\Release\net6.0-windows\win-x64\publish\MobitSystemInfoWidget.exe") do echo %%~zI bytes
    echo.
    echo Ready for NinjaRMM deployment!
) else (
    echo.
    echo Build failed with error code %ERRORLEVEL%
)

pause
