@echo off
echo Building CV Agent Desktop Application...

REM Clean previous builds
if exist "publish" rmdir /s /q "publish"
if exist "dist" rmdir /s /q "dist"

REM Restore packages
echo Restoring packages...
dotnet restore

REM Build the solution
echo Building solution...
dotnet build --configuration Release

REM Publish the desktop application
echo Publishing desktop application...
dotnet publish "src\CVAgentApp.Desktop\CVAgentApp.Desktop.csproj" ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "publish\CVAgentApp.Desktop"

REM Copy additional files
echo Copying additional files...
xcopy "src\CVAgentApp.Desktop\wwwroot" "publish\CVAgentApp.Desktop\wwwroot" /E /I /Y

REM Create distribution folder
echo Creating distribution...
mkdir "dist\CVAgentApp" 2>nul
xcopy "publish\CVAgentApp.Desktop\*" "dist\CVAgentApp\" /E /I /Y

REM Create installer script
echo Creating installer script...
echo @echo off > "dist\install.bat"
echo echo Installing CV Agent Desktop... >> "dist\install.bat"
echo echo. >> "dist\install.bat"
echo echo CV Agent Desktop has been installed to: %CD%\CVAgentApp >> "dist\install.bat"
echo echo. >> "dist\install.bat"
echo echo To run the application, double-click CVAgentApp.Desktop.exe >> "dist\install.bat"
echo echo. >> "dist\install.bat"
echo pause >> "dist\install.bat"

REM Create README for distribution
echo Creating README...
echo CV Agent Desktop - AI-Powered CV Generation > "dist\README.txt"
echo. >> "dist\README.txt"
echo Installation Instructions: >> "dist\README.txt"
echo 1. Run install.bat to install the application >> "dist\README.txt"
echo 2. Double-click CVAgentApp.Desktop.exe to start the application >> "dist\README.txt"
echo. >> "dist\README.txt"
echo System Requirements: >> "dist\README.txt"
echo - Windows 10 or later >> "dist\README.txt"
echo - .NET 9.0 Runtime (included) >> "dist\README.txt"
echo - Internet connection for AI processing >> "dist\README.txt"
echo. >> "dist\README.txt"
echo Features: >> "dist\README.txt"
echo - AI-powered CV analysis >> "dist\README.txt"
echo - Job posting analysis >> "dist\README.txt"
echo - Tailored CV and cover letter generation >> "dist\README.txt"
echo - Company research integration >> "dist\README.txt"
echo - Document preview and download >> "dist\README.txt"

echo.
echo Build completed successfully!
echo.
echo Distribution files are in the 'dist' folder.
echo Run 'dist\install.bat' to install the application.
echo.
pause



