@echo off
echo Building Mobit System Info Widget...
echo.

REM Clean previous builds
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj" rmdir /s /q "obj"

REM Build and publish as regular deployment (not single file)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build completed successfully!
    echo.
    echo Output directory: bin\Release\net6.0-windows\win-x64\publish\
    echo.
    echo Contents:
    dir /b "bin\Release\net6.0-windows\win-x64\publish\"
    echo.
    echo Directory size:
    powershell -Command "'{0:N0} bytes' -f ((Get-ChildItem 'bin\Release\net6.0-windows\win-x64\publish\' -Recurse | Measure-Object -Property Length -Sum).Sum)"
    echo.
    echo Ready to zip for deployment!
    echo.
    echo To create deployment zip:
    echo powershell Compress-Archive -Path "bin\Release\net6.0-windows\win-x64\publish\*" -DestinationPath "MobitSystemInfoWidget.zip" -Force
) else (
    echo.
    echo Build failed with error code %ERRORLEVEL%
)

pause
