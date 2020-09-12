using static StreamDeck.OscBridge.Constants;
using BarRaider.SdTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StreamDeck.OscBridge.Model;

namespace StreamDeck.OscBridge.ViewModel
{
    [PluginActionId("com.barjonas.streamdeck.oscbridge.mediabutton")]
    public class MediaAction : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    GroupIndex = 0,
                    ButtonIndex = 0
                };
                return instance;
            }

            [JsonProperty(PropertyName = "groupIndex")]
            public int GroupIndex { get; set; }

            [JsonProperty(PropertyName = "buttonIndex")]
            public int ButtonIndex { get; set; }
        }

        #region Private Members
        private readonly Sys _sys;
        private readonly PluginSettings _settings;

        private readonly SDConnection _connection;

        #endregion
        public MediaAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            _sys = Sys.s_instance;
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                _settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                _settings = payload.Settings.ToObject<PluginSettings>(s_jsonTolerantDeserializer);
            }
            _connection = connection;

            EnsureBinding();
        }

        int? _groupIndex = null;
        int? _actionIndex = null;
        private MediaItem _model;

        /// <summary>
        /// Ensure this instance is bound to the right DefaultButton in the model
        /// </summary>
        private void EnsureBinding()
        {            
            if (_settings?.GroupIndex != _groupIndex || _settings?.ButtonIndex != _actionIndex)
            {
                MediaItem newModel = _settings == null ? null : _sys.GetMediaItem(_settings.GroupIndex, _settings.ButtonIndex);
                if (newModel != _model)
                {
                    if (_model != null)
                    {
                        _model.BackgroundChanged -= UpdateImage;
                        _model.LabelChanged -= UpdateLabel;
                    }
                    _model = newModel;
                    if (_model != null)
                    {
                        _model.BackgroundChanged += UpdateImage;
                        _model.LabelChanged += UpdateLabel;
                    }
                    UpdateImage();
                    UpdateLabel();                        
                }
                _groupIndex = _settings?.GroupIndex;
                _actionIndex = _settings?.ButtonIndex;
            }            
        }

        private void UpdateImage()
        {
            _connection?.SetImageAsync(_model?.Background ?? string.Empty);
        }

        private void UpdateLabel()
        {
            _connection?.SetTitleAsync(_model?.Label ?? string.Empty);
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            if (_groupIndex.HasValue && _actionIndex.HasValue && _settings != null && !string.IsNullOrWhiteSpace(_model.FilePath))
            {
                _sys.Osc.MediaRequest(_settings.GroupIndex, _model.FilePath);
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, "Default action Pressed");
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            try
            {
                Tools.AutoPopulateSettings(_settings, payload.Settings);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Exception while populating setttings: {ex.Message}");
            }
            SaveSettings();
            EnsureBinding();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(_settings));
        }

        #endregion
    }
}
