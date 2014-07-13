@echo off
rem ===================================================
rem Cтроит решение и (при успехе) деплоит базу
rem ===================================================

echo Building solution...
"%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" /build Debug Platform.sln

IF ERRORLEVEL 1 GOTO error

echo Build was completed successfully. Starting Deploy...
cd Tools.MigrationHelper
@echo on
DeployDb.bat
@echo off
pause
GOTO exit 

:error
echo Building failed.  Exit without performing Deploy.
pause
EXIT %ERRORLEVEL%

:exit
