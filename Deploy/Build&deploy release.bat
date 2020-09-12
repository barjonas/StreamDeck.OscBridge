call "%~dp0set variables.bat"
cd "%~dp0..\Plugin"
dotnet publish -c Release --self-contained false

SET buildDir=%~dp0..\Plugin\bin\Release\%pluginName%
SET zip="%programfiles%\7-Zip\7z.exe"
SET OUTPUT_DIR="%~dp0Output"
SET DISTRIBUTION_TOOL="%~dp0DistributionTool.exe"

taskkill /f /im streamdeck.exe
taskkill /f /im %assyName%.exe
timeout /t 2
rmdir /S /Q %OUTPUT_DIR%
md %OUTPUT_DIR%
del "%buildDir%\*.pdb"
%DISTRIBUTION_TOOL% -b -i "%buildDir%" -o %OUTPUT_DIR%

robocopy "%~dp0..\Demo\Ventuz\Archives" "%OUTPUT_DIR%\Ventuz 6" /MIR
robocopy "%~dp0..\Demo\Ventuz\Repository\StreamDeck.OscBridge" "%OUTPUT_DIR%\Ventuz 6\Repository" /MIR
robocopy "%~dp0..\Demo\Ventuz 5\Archives" "%OUTPUT_DIR%\Ventuz 5" /MIR
robocopy "%~dp0..\Demo\Ventuz 5\Repository\StreamDeck.OscBridge" "%OUTPUT_DIR%\Ventuz 5\Repository" /MIR
del %assyName%.zip
%zip% a -tzip -y -r %assyName%.zip "%OUTPUT_DIR%/*.*"


rmdir /S /Q %productionDir%
START "" %STREAM_DECK_FILE% "%OUTPUT_DIR%\%compiledPluginName%"