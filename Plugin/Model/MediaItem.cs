using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StreamDeck.OscBridge.Model
{
    internal class MediaItem
    {
        internal event Action LabelChanged;
        internal event Action BackgroundChanged;

        internal void Update(string path, bool forceLabel)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                FilePath = string.Empty;
                Label = string.Empty;                
                Background = ViewModel.IconManager.s_instance.GetImage(string.Empty, out _);
            }
            else
            {
                FilePath = path;
                Background = ViewModel.IconManager.s_instance.GetImage(path, out bool usingFallback);
                Label = forceLabel || usingFallback ? Path.GetFileNameWithoutExtension(path) : string.Empty;
            }
        }

        private string _background;
        internal string Background
        {
            get
            {
                return _background;
            }
            set
            {
                if (_background != value)
                {
                    _background = value;
                    BackgroundChanged?.Invoke();
                }
            }
        }

        private string _label;
        internal string Label
        {
            get
            {
                return _label;
            }
            set
            {
                if (_label != value)
                {
                    _label = value;
                    LabelChanged?.Invoke();
                }
            }
        }

        internal string FilePath { get; private set; }
    }
}
