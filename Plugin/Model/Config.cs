using System;
using System.Collections.Generic;
using System.Text;

namespace StreamDeck.OscBridge.Model
{
    internal class Config
    {
        internal event Action IconDirectoryChanged;
        internal event Action BackgroundDirectoryChanged;
        internal event Action RequestPortChanged;

        private string _iconDirectory;
        internal string IconDirectory
        {
            get
            {
                return _iconDirectory;
            }
            set
            {
                if (_iconDirectory != value)
                {
                    _iconDirectory = value;
                    IconDirectoryChanged?.Invoke();
                }
            }
        }

        private string _backgroundDirectory;
        internal string BackgroundDirectory
        {
            get
            {
                return _backgroundDirectory;
            }
            set
            {
                if (_backgroundDirectory != value)
                {
                    _backgroundDirectory = value;
                    BackgroundDirectoryChanged?.Invoke();
                }
            }
        }

        private string _ffMpegDirectory;
        internal string FfMpegDirectory
        {
            get
            {
                return _ffMpegDirectory;
            }
            set
            {
                if (_ffMpegDirectory != value)
                    _ffMpegDirectory = value;
            }
        }

        private int _sendPort;
        internal int RequestPort
        {
            get
            {
                return _sendPort;
            }
            set
            {
                if (_sendPort != value)
                {
                    _sendPort = value;
                    RequestPortChanged?.Invoke();
                }
            }
        }

    }
}
