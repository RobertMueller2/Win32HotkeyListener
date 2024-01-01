using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Win32HotkeyListener.Win32;

namespace Win32HotkeyListener {
    public abstract class BaseHotkey {

        [XmlIgnore]
        public BaseAction Action { get; set; }

        [XmlElement("KeyCombination", typeof(KeyCombination))]
        public KeyCombination KeyCombo { get; set; }

        [XmlIgnore]
        public User32Hotkey RealHotkey { get; set; }

        private bool _enabled = false;

        [XmlIgnore]
        public bool Enabled {
            get => _enabled;
            set {
                if (!Registered) {
                    return;
                }
                _enabled = value;
            }
        }

        private bool _registered = false;
        [XmlIgnore]
        public bool Registered {
            get => _registered;
            private set {
                _registered = value;
                Enabled = value;
            }
        }

        public override string ToString() {
            return $"{this.GetType().Name} : {KeyCombo.ToString()}";
        }

        internal bool TryRegister(uint id) {
            RealHotkey = new User32Hotkey(id, KeyCombo.ModifierCombo, KeyCombo.Key.Keycode);

            if (!RealHotkey.Initialised) {
                RealHotkey.Dispose();
                return false;
            }

            Registered = true;
            return true;
        }

        internal void Unregister() {
            if (!Registered) {
                return;
            }
            RealHotkey.Dispose();            
        }
    }
}