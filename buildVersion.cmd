PATH %TC_DOTNET_FRAMEWORK_PATH%;%PATH%
msbuild @Platform.Web\Platform.MSBuild.args Platform.Web\Platform.MSBuild.proj
IF ERRORLEVEL 1 GOTO error

GOTO exit

:error
EXIT %ERRORLEVEL%

:exit
