using System;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Win32HotkeyListener {
    public class Key {

        [XmlIgnore]
        public Keys WFKey { get; set; }

        [XmlIgnore]
        public uint Keycode {
            get {
                return (uint)WFKey;
            }
        }

        [XmlIgnore]
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
