@echo off
cls
call settings.bat
java -jar bin/JsTestDriver.jar --port 9876 --config jsTestDriver.conf --browser %FIREFOX% --testOutput "logs" --tests all --verbose --captureConsole
