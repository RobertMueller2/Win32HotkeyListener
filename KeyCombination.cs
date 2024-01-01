using System;
using System.Collections.Generic;
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
        public Key Key { get; set; }

        /// <summary>
        /// The modifiers of the combination.
        /// </summary>
        [XmlElement("Modifier")]
        public List<string> Modifiers { get; set; }

        /// <summary>
        /// Return modifier keycode as uint.
        /// </summary>
        [XmlIgnore]
        public uint ModifierCombo {
            get {
                uint mod = (uint)ModifierKeys.NONE;
                foreach (var modifier in Modifiers) {
                    mod += (uint)Enum.Parse(typeof(ModifierKeys), modifier, true);
                }
                return mod;

            }
        }

        /// <summary>
        /// Overridden ToString method for use in logging.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{string.Join("+", Modifiers).Coalesce("")}+{Key.KeyStr.ToString()}";
    }

}
