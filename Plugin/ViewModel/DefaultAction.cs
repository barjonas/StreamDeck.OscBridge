using static StreamDeck.OscBridge.Constants;
using System;
using System.Threading.Tasks;
using BarRaider.SdTools;
using StreamDeck.OscBridge.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StreamDeck.OscBridge.ViewModel
{
    [PluginActionId("com.barjonas.streamdeck.oscbridge.defaultbutton")]
    public class DefaultAction : PluginBase
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

            private int _groupIndex;
            [JsonProperty(PropertyName = "groupIndex")]
            public int GroupIndex { get { return _groupIndex; } set { _groupIndex = value < 0 ? 0 : value; } }

            [JsonProperty(PropertyName = "buttonIndex")]
            public int ButtonIndex { get; set; }
        }

        #region Private Members
        private readonly Sys _sys;
        private readonly PluginSettings _settings;

        private readonly SDConnection _connection;

        #endregion
        public DefaultAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
        private DefaultButton _model;

        /// <summary>
        /// Ensure this instance is bound to the right DefaultButton in the model
        /// </summary>
        private void EnsureBinding()
        {
            if (_settings?.GroupIndex != _groupIndex || _settings?.ButtonIndex != _actionIndex)
            {
                DefaultButton newModel = _settings == null ? null : _sys.GetDefaultButton(_settings.GroupIndex, _settings.ButtonIndex);
                if (newModel != _model)
                {
                    if (_model != null)
                    {
                        _model.IconChanged -= UpdateImage;
                        _model.BackgroundChanged -= UpdateImage;
                        _model.LabelChanged -= UpdateLabel;
                    }
                    _model = newModel;
                    if (_model != null)
                    {
                        _model.IconChanged += UpdateImage;
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
            _connection?.SetImageAsync(IconManager.s_instance.GetImage(_model?.Icon, _model?.Background));
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
            if (_groupIndex.HasValue && _actionIndex.HasValue)
            {
                _sys.Osc.DefaultRequest(_groupIndex.Value, _actionIndex.Value);
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
