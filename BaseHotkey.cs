using System.Xml.Serialization;
using Win32HotkeyListener.Win32;

namespace Win32HotkeyListener {

    /// <summary>
    /// Base class for hotkeys. The main reason for it to be abstract is to allow for the Action property to be overridden so it can be serialised with whatever derived classes for BaseAction are used.
    /// </summary>
    public abstract class BaseHotkey {

        /// <summary>
        /// Action to be performed when the hotkey is pressed.
        /// </summary>
        [XmlIgnore]
        public BaseAction Action { get; set; }

        /// <summary>
        /// A key combination that triggers the hotkey
        /// </summary>
        [XmlElement("KeyCombination", typeof(KeyCombination))]
        public KeyCombination KeyCombo { get; set; }

        /// <summary>
        /// The real hotkey object which represents a hotkey that was registered with the OS.
        /// </summary>
        [XmlIgnore]
        public User32Hotkey RealHotkey { get; set; }

        /// <summary>
        /// Whether the hotkey is enabled or not.
        /// </summary>
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

        /// <summary>
        /// Whether the hotkey is registered or not. Only registered hotkeys can be enabled, and only enabled hotkeys have to be disposed.
        /// </summary>
        private bool _registered = false;
        [XmlIgnore]
        public bool Registered {
            get => _registered;
            private set {
                _registered = value;
                Enabled = value;
            }
        }

        /// <summary>
        /// Overridden ToString method for use in logging.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"{this.GetType().Name} : {KeyCombo.ToString()}";
        }

        /// <summary>
        /// Trys to register the hotkey with the OS.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>whether registration was successful</returns>
        internal bool TryRegister(uint id) {
            RealHotkey = new User32Hotkey(id, KeyCombo.ModifierCombo, KeyCombo.Key.Keycode);

            if (!RealHotkey.Initialised) {
                RealHotkey.Dispose();
                return false;
            }

            Registered = true;
            return true;
        }

        /// <summary>
        /// Unregisters the hotkey with the OS.
        /// </summary>
        internal void Unregister() {
            if (!Registered) {
                return;
            }
            RealHotkey.Dispose();
        }
    }
}