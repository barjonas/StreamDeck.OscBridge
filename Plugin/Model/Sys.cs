using static StreamDeck.OscBridge.Constants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StreamDeck.OscBridge.Model
{
    internal class Sys : IDisposable
    {
        internal readonly static Sys s_instance = new Sys();

        private Sys()
        {
            Config = new Config();
            DefaultButtons = new ConcurrentDictionary<Tuple<int, int>, DefaultButton>();
            MediaButtonSets = new ConcurrentDictionary<int, MediaButtonSet>();
            LoadDefaults();
            Osc = new OscWrapper(this);            
        }

        private void LoadDefaults()
        {
            //Load initial values. Todo: load these from file and save at end.
            string s_dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Barjonas", "StreamDeck.OscBridge");
            Config.FfMpegDirectory = s_ffMpegDefaultPath;
            Config.BackgroundDirectory = Path.Combine(s_dataDir, "Backgrounds");
            Config.IconDirectory = Path.Combine(s_dataDir, "Icons");
            Config.RequestPort = s_oscSendEndPoint.Port;
        }

        internal OscWrapper Osc { get; }
        public Config Config { get; }
        private ConcurrentDictionary<Tuple<int, int>, DefaultButton> DefaultButtons { get; }
        public ConcurrentDictionary<int, MediaButtonSet> MediaButtonSets { get; }

        private static Tuple<int, int> BuildKey(int group, int button) => new Tuple<int, int>(group, button);

        internal DefaultButton GetDefaultButton(int group, int button)
        {
            return DefaultButtons.GetOrAdd(BuildKey(group, button), (i) => new DefaultButton());
        }

        internal MediaItem GetMediaItem(int group, int button)
        {
            return MediaButtonSets.GetOrAdd(group, (i) => new MediaButtonSet()).Items.ElementAtOrDefault(button);
        }

        public void Dispose()
        {
            Osc?.Dispose();
        }
    }
}
