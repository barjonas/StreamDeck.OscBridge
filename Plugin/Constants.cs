using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace StreamDeck.OscBridge
{
    internal static class Constants
    {
        internal const int Columns = 5;
        internal const int Rows = 3;
        internal const int ButtonImageWidth = 72;
        internal const int ButtonImageHeight = 72;
        internal const int MaxMediaItemsPerSet = Columns * Rows;
        internal const string OscServerRootConfig = "Config";
        internal const string OscRequestRefresh = "RequestRefresh";
        internal const string OscRootDefaultButtons = "DefaultButtons";
        internal const string OscRootMediaButtonSets = "MediaButtonSets";
        internal const string OscButtonRequested = "Requested";
        internal const string IconKeyDefault = "keyDefault.png";
        internal const string IconKeyMedia = "keyMedia.png";
        internal const string IconKeyMediaMissing = "keyMediaMissing.png";
        internal static readonly string s_ffMpegDefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"ffmpeg\bin");
        internal static readonly IPEndPoint s_oscSendEndPoint = new IPEndPoint(IPAddress.Loopback, 7822);
        internal static readonly IPEndPoint s_oscRecieveEndPoint = new IPEndPoint(IPAddress.Loopback, 7823);
        internal static readonly JsonSerializer s_jsonTolerantDeserializer = JsonSerializer.CreateDefault(new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
    }
}
