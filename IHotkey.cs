using System.Xml.Serialization;

namespace Win32HotkeyListener {
    public interface IHotkey {

        BaseAction Action { get; set; }

        KeyCombination KeyCombo { get; set; }

        bool Enabled { get; set; }

        bool Registered { get; set; }

    }
}