using static StreamDeck.OscBridge.Constants;
using Rug.Osc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using StreamDeck.OscBridge.Model;
using BarRaider.SdTools;
using System.Threading.Tasks;

namespace StreamDeck.OscBridge.Model
{
    internal class OscWrapper : IDisposable
    {
        private OscSender _client;
        private readonly OscReceiver _server;
        private readonly Sys _sys;
        public OscWrapper(Sys sys)
        {
            _sys = sys;
            InitializeClient();
            _server = new OscReceiver(s_oscRecieveEndPoint.Address, s_oscRecieveEndPoint.Port);
            _server.Connect();
            Task.Run(() => ListenLoop());
        }

        private void InitializeClient()
        {
            if (_client != null)
            {
                _client?.Close();
                _client?.Dispose();
            }
            _client = new OscSender(IPAddress.Any, IPAddress.Loopback, _sys.Config.RequestPort);
            _client.Connect();
        }

        private void ListenLoop()
        {            
            try
            {
                while (_server.State != OscSocketState.Closed)
                {
                    if (_server.State == OscSocketState.Connected)
                    {
                        // get the next message 
                        // this will block until one arrives or the socket is closed
                        OscPacket packet = _server.Receive();
                        if (packet.Error != OscPacketError.None)
                        {
                            Logger.Instance.LogMessage(TracingLevel.ERROR, $"Recieved corrupt OSC packet: {packet.ErrorMessage}");
                        }
                        switch (packet)
                        {
                            case OscBundle b:
                                foreach (OscMessage m in b)
                                {
                                    ProcessOscMessage(m);
                                }
                                break;
                            case OscMessage m:
                                ProcessOscMessage(m);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // if the socket was connected when this happens
                // then tell the user
                if (_server.State == OscSocketState.Connected)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"Exception in OSC listen loop: {ex.Message}");
                }
            }
        }

        private void ProcessOscMessage(OscMessage message)
        {
            if (message.Count != 1)
            {
                return;
            }
            string[] parts = message.Address.Split('/');
            switch (parts[1])
            {
                case OscServerRootConfig:
                    if (parts.Length == 3)
                    {
                        switch (parts[2])
                        {
                            case nameof(Config.IconDirectory):
                                if (HandleUnicode(message[0]) is string id)
                                {
                                    _sys.Config.IconDirectory = id;
                                }
                                break;
                            case nameof(Config.BackgroundDirectory):
                                if (HandleUnicode(message[0]) is string bd)
                                {
                                    _sys.Config.BackgroundDirectory = bd;
                                }
                                break;
                            case nameof(Config.RequestPort):
                                if (message[0] is int p)
                                {
                                    _sys.Config.RequestPort = p;
                                    InitializeClient();
                                }
                                break;
                        }
                    }
                    break;
                case OscRootDefaultButtons:
                case OscRootMediaButtonSets:                    
                    if (int.TryParse(parts[2], out int index))
                    {
                        if (HandleUnicode(message[0]) is string vs)
                        {
                            if (parts[1] == OscRootMediaButtonSets)
                            {
                                if (parts.Length == 4)
                                {
                                    SetMediaButtonSetsProperty(index, parts[3], vs);
                                }
                            }
                            else
                            {
                                if (parts.Length == 5 && int.TryParse(parts[3], out int subIndex))
                                {
                                    SetDefaultButtonProperty(index, subIndex, parts[4], vs);
                                }
                            }
                        }
                    }                    
                    break;
            }
        }

        private static object HandleUnicode(object value)
        {
            if (value is byte[] bytes)
            {
                //reverse pairs
                byte[] reversed = new byte[bytes.Length - 4];
                for (int i = 0; i < reversed.GetUpperBound(0); i += 2)
                {
                    reversed[i] = bytes[i + 5];
                    reversed[i + 1] = bytes[i + 4];
                }
                return Encoding.Unicode.GetString(reversed);
            }
            return value;
        }

        private void SetDefaultButtonProperty(int groupIndex, int buttonIndex, string property, string value)
        {
            DefaultButton b = _sys.GetDefaultButton(groupIndex, buttonIndex);
            switch (property)
            {
                case nameof(DefaultButton.Background):
                    b.Background = value;
                    return;
                case nameof(DefaultButton.Icon):
                    b.Icon = value;
                    return;
                case nameof(DefaultButton.Label):
                    b.Label = value;
                    return;
            }
        }

        private void SetMediaButtonSetsProperty(int index, string property, string value)
        {
            MediaButtonSet b = _sys.MediaButtonSets.GetOrAdd(index, (i) => new MediaButtonSet());
            switch (property)
            {
                case nameof(MediaButtonSet.DirectoryPath):
                    b.DirectoryPath = value;
                    return;
                case nameof(MediaButtonSet.Filter):
                    b.Filter = value;
                    return;
            }
        }

        internal void RequestRefresh()
        {
            _client.Send(new OscMessage($"/{OscRequestRefresh}"));
        }

        internal void DefaultRequest(int groupIndex, int buttonIndex)
        {
            _client.Send(new OscMessage($"/{OscRootDefaultButtons}/{groupIndex}/{buttonIndex}"));
        }

        internal void MediaRequest(int buttonSet, string path)
        {
            _client.Send(new OscMessage($"/{OscRootMediaButtonSets}/{buttonSet}", path));
        }

        public void Dispose()
        {
            _client?.Close();
            _client?.Dispose();
            _server.Close();
        }
    }
}
