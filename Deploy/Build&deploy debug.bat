call "%~dp0set variables.bat"
cd "%~dp0..\Plugin"
dotnet publish -c Debug --self-contained false

md "%localappdata%\Barjonas\StreamDeck.OscBridge"
robocopy %~dp0..\ImageExamples "%localappdata%\Barjonas\StreamDeck.OscBridge" -MIR 

taskkill /f /im streamdeck.exe
taskkill /f /im %assyName%.exe
timeout /t 2
robocopy %~dp0..\Plugin\bin\Debug\%pluginName% %productionDir% -MIR
START "" %STREAM_DECK_FILE%

pause