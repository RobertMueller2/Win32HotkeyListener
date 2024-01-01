using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using static Win32HotkeyListener.KeyCombination;

namespace Win32HotkeyListener.Win32 {

    public class User32Hotkey : IDisposable {

        // Registers a hotkey with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // Unregisters the hotkey with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public uint Id { get; private set; }
        public bool Initialised { get; private set; }

        public User32Hotkey(uint id, uint modifiers, uint keycode) {
            this.Id = id;

            if (!RegisterHotKey(IntPtr.Zero, checked((int)id), modifiers, keycode)) {
                Logger.GetInstance().WriteLine("Warning: Key already registered. {0:X}|{1:X}", ((ModifierKeys)modifiers).ToString(), ((Keys)keycode).ToString());
                return;
            }

            Logger.GetInstance().WriteLine("Registered id {0:X}|modifier {1:X}|keycode {2:X}", id, ((ModifierKeys)modifiers).ToString(), ((Keys)keycode).ToString());
            Initialised = true;
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (Initialised) {
                var result = UnregisterHotKey(IntPtr.Zero, checked((int)Id));

                if (!disposing) {
                    return;
                }

                var error = "";
                if (!result) {
                    error = Marshal.GetLastWin32Error().ToString();
                }
                Console.WriteLine("Unregistered id {0:X}: {1}|{2}", Id, result, error);
            }
        }

    }
}