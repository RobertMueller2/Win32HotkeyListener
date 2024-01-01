using System.Xml.Serialization;

namespace Win32HotkeyListener {
    public abstract class BaseHotkey {

        [XmlIgnore]
        public BaseAction Action { get; set; }

        [XmlElement("KeyCombination", typeof(KeyCombination))]
        public KeyCombination KeyCombo { get; set; }

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
            set {
                _registered = value;
                Enabled = value;
            }
        }

        public override string ToString() {
            return $"{this.GetType().Name} : {KeyCombo.ToString()}";
        }
    }
}