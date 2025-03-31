using System.Xml.Serialization;
using static Win32HotkeyListener.Win32.User32Hotkey;

namespace Win32HotkeyListener {

    /// <summary>
    /// Represents a key combination, consisting of a key and a list of modifiers.
    /// The separation is needed because of the way the OS handles hotkeys.
    /// <see cref="Win32.User32Hotkey"/>
    /// </summary>
    public class KeyCombination {

        /// <summary>
        /// The key of the combination.
        /// </summary>
        [XmlElement("Key")]
        public Key? Key { get; set; }

        private List<string> _modifiers = new List<string>();
        /// <summary>
        /// The modifiers of the combination.
        /// 
        /// </summary>
        [XmlElement("Modifier")]
        public List<string> Modifiers { 
            get => _modifiers;
            set {
                ModifierComboFromList(value); // should throw exception if invalid
                _modifiers = value;
            }
        }

        /// <summary>
        /// Return modifier keycode as uint.
        /// </summary>
        [XmlIgnore]
        public uint ModifierCombo {
            get => ModifierComboFromList(Modifiers);
        }

        public static uint ModifierComboFromList(List<string> modifiers) {
            uint mod = (uint)ModifierKeys.NONE;
            foreach (var modifier in modifiers) {
                mod += (uint)Enum.Parse(typeof(ModifierKeys), modifier, true);
            }
            return mod;
        }

        /// <summary>
        /// Overridden ToString method for use in logging.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{string.Join("+", Modifiers).Coalesce("")}+{Key?.KeyStr.ToString()}";
    }

}
