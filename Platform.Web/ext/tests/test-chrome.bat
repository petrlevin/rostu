@echo off
cls
call settings.bat
java -jar bin/JsTestDriver.jar --port 9876 --config jsTestDriver.conf --browser %CHROME% --testOutput "logs" --tests all --verbose --captureConsole
