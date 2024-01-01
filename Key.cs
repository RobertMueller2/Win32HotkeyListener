using System;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Win32HotkeyListener {

    /// <summary>
    /// Wrapper class for System.Windows.Forms.Keys to allow for XML serialisation.
    /// Note that Keys contains both keys and modifiers, but only keys are used
    /// for the HotkeyListener. Modifiers have a separate representation in
    /// <see cref="KeyCombination"/>.
    /// 
    /// It can still be useful that modifiers are available, should keys be serialised
    /// for purposes other than a hotkey, so no limitations are being made here.
    /// </summary>
    public class Key {

        /// <summary>
        /// A key from <see cref="System.Windows.Forms.Keys"/>.
        /// </summary>
        [XmlIgnore]
        public Keys WFKey { get; set; }

        /// <summary>
        /// The keycode of the key.
        /// </summary>
        [XmlIgnore]
        public uint Keycode {
            get {
                return (uint)WFKey;
            }
        }

        /// <summary>
        /// Provides a string representation of the key both for retrieval and setting the key.
        /// </summary>
        [XmlText]
        public string KeyStr {
            get {
                return WFKey.ToString();
            }
            set {
                //FIXME error handling?
                WFKey = (Keys)Enum.Parse(typeof(Keys), value, true);
            }
        }

    }
}
