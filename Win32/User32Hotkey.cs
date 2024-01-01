using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using static Win32HotkeyListener.KeyCombination;

namespace Win32HotkeyListener.Win32 {

    /// <summary>
    /// Provides user32.dll functions for registering and unregistering hotkeys and represents a hotkey registered with the OS.
    /// </summary>
    public class User32Hotkey : IDisposable {

        /// <summary>
        /// Enum for key modifiers.
        /// </summary>
        [Flags]
        public enum ModifierKeys : uint {
            NONE = 0,
            ALT = 0x0001,
            CTRL = 0x0002,
            SHIFT = 0x0004,
            WIN = 0x0008,
            MOD_NOREPEAT = 0x4000
        }

        /// <summary>
        /// Unmanaged function for registering a hotkey with Windows from user32.dll.
        /// 
        /// <see href="https://learn.microsoft.com/de-de/windows/win32/api/winuser/nf-winuser-registerhotkey"/>
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="id"></param>
        /// <param name="fsModifiers"></param>
        /// <param name="vk"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        /// <summary>
        /// Unmanaged function for unregistering a hotkey with Windows from user32.dll.
        /// 
        /// <see href="https://learn.microsoft.com/de-de/windows/win32/api/winuser/nf-winuser-unregisterhotkey"/>
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="id"></param>
        /// <returns></returns>        
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// The hotkey id, needs to be tracked here for unregistering
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Whether the hotkey was registered yet.
        /// </summary>
        public bool Initialised { get; private set; }

        /// <summary>
        /// Constructor for a OS hotkey representation.
        /// </summary>
        /// <param name="id">the hotkey id</param>
        /// <param name="modifiers">key modifiers</param>
        /// <param name="keycode">the key</param>
        public User32Hotkey(uint id, uint modifiers, uint keycode) {
            this.Id = id;

            if (!RegisterHotKey(IntPtr.Zero, checked((int)id), modifiers, keycode)) {
                Logger.GetInstance().WriteLine("Warning: Key already registered. {0:X}|{1:X}", ((ModifierKeys)modifiers).ToString(), ((Keys)keycode).ToString());
                return;
            }

            Logger.GetInstance().WriteLine("Registered id {0:X}|modifier {1:X}|keycode {2:X}", id, ((ModifierKeys)modifiers).ToString(), ((Keys)keycode).ToString());
            Initialised = true;
        }
        
        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
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
                Logger.GetInstance().WriteLine("Unregistered id {0:X}: {1}|{2}", Id, result, error);
            }
        }

    }
}