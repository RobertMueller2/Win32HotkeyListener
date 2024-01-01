using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Win32HotkeyListener {
    public class KeyCombination {

        [Flags]
        public enum ModifierKeys : uint {
            NONE = 0,
            ALT = 0x0001,
            CTRL = 0x0002,
            SHIFT = 0x0004,
            WIN = 0x0008,
            MOD_NOREPEAT = 0x4000
        }

        [XmlElement("Key")]
        public Key Key { get; set; }

        [XmlElement("Modifier")]
        public List<string> Modifiers { get; set; }

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

        public override string ToString() => $"{string.Join("+", Modifiers).Coalesce("")}+{Key.KeyStr.ToString()}";
    }

}
