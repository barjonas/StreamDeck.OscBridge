using System;
using System.Collections.Generic;
using System.Text;

namespace StreamDeck.OscBridge.Model
{
    internal class DefaultButton
    {
        internal event Action IconChanged;
        internal event Action BackgroundChanged;
        internal event Action LabelChanged;

        private string _icon;
        internal string Icon {
            get
            {
                return _icon;
            }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    IconChanged?.Invoke();
                }
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
    }
}
