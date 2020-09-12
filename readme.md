# Stream Deck OSC Bridge ###
## Introduction ##
This is a plug in for Stream Deck to allow bidirectional communication over [OSC][1]. It is currently compatible only with Stream Deck running on Windows. This allows control of and feedback from any system which allows you to send and receive [OSC][1]. It's currently designed for the OSC Master to be running on the same machine. The original use-case was to run it on a machine that was also running a [Ventuz][2] presentation. Two different button types are implemented:
### DefaultButton ###
The OSC master can set the background, icon and label of each button. The plugin will send a message to the OSC master each time the button is pressed.
### MediaButton ###
The OSC master can provide a path to a directory containing media for each group of Media Buttons. Each image or movie the plugin finds in the directory will be displayed on a button in the group. Whenever a button is pressed, the path of its associated media is sent to the OSC master.
## Installation ##
### Prerequisites ###
This plugin is compiled as a .NET Core 3.1 [framework-dependent executable][6]. That means you will need to have the [.NET Core 3.1 Desktop Runtime][7] for your platform installed before installing the plugin. 
To display frames from movies, the plugin expects to find `ffmpeg.exe` and `ffprobe.exe` at `%programfiles%\ffmpeg\bin`. You can get these from any good public FFmpeg build such as [these linked on the FFmpeg site][5].
### Importing into Stream Deck ###
When built and packaged, this project ends up as a single file called `com.barjonas.streamdeck.oscbridge.streamDeckPlugin`. By default, this file type is associated with `StreamDeck.exe`, which will use this file to install the plugin.
## Demo ##
The demo includes projects for [Ventuz][2] versions 5 and 6. Logic for two groups of radio buttons is implemented within Ventuz using the default type Stream Deck buttons. The backgrounds of the groups are changed according to the current selection within each group. Other buttons are configured as simple toggles. Directories of movies and images are parsed as presented on the Stream Deck buttons. When any of these is pressed, the associated media is displayed on the Ventuz video output.
Ventuz project defines the directories from which all media should be pulled are within `C:\Users\msa\AppData\Local\Barjonas\StreamDeck.OscBridge`. The `Backgrounds` and `Icons` directories can be copied from `ImageExamples` in the project. `Images` should be filled with 10 images of your choice. `Movies` should be filled with several movie files.
## Implementing OSC Master ##
To use the plugin as-is, you can simply interact with it through OSC. All OSC messages should be sent to localhost (127.0.0.1) port 7823. All strings are expected to be Unicode, encoded in an OSC Blob. The addresses that the plugin listens to are listed here:
Address | Type | Example value | Description
------- | ------------- | -----------
/Config/RequestPort | Integer| 7822 | The port to which messages from the plugin will be sent.
/Config/BackgroundDirectory | String | %localappdata%\Barjonas\StreamDeck.OscBridge\Backgrounds | The directory in which the plugin should look for background images.
/Config/IconDirectory | String | %localappdata%\Barjonas\StreamDeck.OscBridge\Icons | The directory in which the plugin should look for foreground images.
/DefaultButtons/{g}/{i}/Icon | String | take | The name of the foreground image to show on button {i} of group {g}. This should be the name of a file in the IconDirectory above, without any file extension.
/DefaultButtons/{g}/{i}/Background | String | program | The name of the background image to show on button {i} of group {g}. This should be the name of a file in the BackgroundDirectory above, without any file extension.
/DefaultButtons/{g}/{i}/Label | String | Camera 1 | A label to be shown on button {i} of group {g}.
/MediaButtonSets/{g}/DirectoryPath | String | %localappdata%\Barjonas\StreamDeck.OscBridge\Movies | A directory from which Media Button group {g} should be populated.
/MediaButtonSets/0 | String | *.mp4|*.mov|*.avi|*.mkv | A pipe-separated list of files extensions which are allowed in this group.

The address that the plugin sends to are:
Address | Type | Example value | Description
------- | ------------- | -----------
/RequestRefresh | Void | | Send by the plugin on startup. The OSC master should respond by resending all values.
/DefaultButtons/{g}/{i}| Void | | A button with index {i} in group {g} was pressed.
/MediaButtonSets/{g} | String | C:\Users\user\AppData\Local\Barjonas\StreamDeck.OscBridge\Movies\GhostbustersII.mp4 | A button in group {g} was pressed. The string contains the absolute path to that button's associated media.

## Thanks and dependencies ##
This project relies heavily on [streamdeck-tools][8] by [BarRaider][9], which solves many of the complexites of consuming the Stream Deck SDK from .NET. You can find some chunks of that project's sample code in this project. Some image processing happens in there. All other image processing happens in the excellent [ImageSharp][10] from [SixLabors][11]. OSC operations are wrapped by [StarDust.RugOsc.netstandard][12] which is a fork of [Rug.Osc][13]. I can make it work for most things I need, but there's room for improvement around its support of broadcast, unicast and port sharing between multiple applications on the same OS instance.

## Debugging ##
There is a batch file which can be used to build and install the plugin for debugging. When Stream Box starts, the plugin will wait for the debugger to be attach before continuing. It expects `dotnet` to be available on the path.

## Deploying from source ##
There is a batch file which can be used to build and pack the plugin ready for distribution. It requires [7zip][2] to be installed and the [Elegato distribution tool][4] to be placed in the `Deploy` directory. It expects `dotnet` to be available on the path.

## License ##
This project is licensed under the very permissive MIT License. You should not modify any copyright notices, but you may add your own if you make modifications.

[1]:http://opensoundcontrol.org/introduction-osc
[2]:https://www.ventuz.com
[3]:https://www.7-zip.org/
[4]:https://developer.elgato.com/documentation/stream-deck/distributiontool/DistributionToolWindows.zip
[5]:https://ffmpeg.org/download.html#build-windows
[6]:https://docs.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli#framework-dependent-executable
[7]:https://dotnet.microsoft.com/download/dotnet-core/3.1
[8]:https://github.com/BarRaider/streamdeck-tools
[9]:https://github.com/BarRaider
[10]:https://github.com/SixLabors/ImageSharp
[11]:https://github.com/SixLabors
[12]:https://github.com/dust63/StarDust.RugOsc.netstandard
[13]:https://bitbucket.org/rugcode/rug.osc/src/master/