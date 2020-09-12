using static StreamDeck.OscBridge.Constants;
using BarRaider.SdTools;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Linq;
using StreamDeck.OscBridge.Model;

namespace StreamDeck.OscBridge
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }            
#endif
            ViewModel.IconManager.s_instance.NoOperation();
            Sys.s_instance.Osc.RequestRefresh();
            SDWrapper.Run(args);
            Sys.s_instance.Dispose();
        }
    }
}
