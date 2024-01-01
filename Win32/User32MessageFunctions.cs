using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Win32HotkeyListener.Win32 {
    internal static class User32MessageFunctions {

        /// <summary>
        /// Message structure for GetMessageA and PeekMessageA
        /// </summary>
        internal enum MsgType : int {
            WM_QUIT = 0x0012,
            WM_HOTKEY = 0x0312
        };

        /// <summary>
        /// Unmanaged function for getting a message from the message queue from user32.dll.
        /// 
        /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessagea"/> 
        /// </summary>
        /// <param name="lpMsg"></param>
        /// <param name="hWnd"></param>
        /// <param name="wMsgFilterMin"></param>
        /// <param name="wMsgFilterMax"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern int GetMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        /// <summary>
        /// Unmanaged function for peeking a message from the message queue from user32.dll.
        /// 
        /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-peekmessagea"/>
        /// </summary>
        /// <param name="lpMsg"></param>
        /// <param name="hWnd"></param>
        /// <param name="wMsgFilterMin"></param>
        /// <param name="wMsgFilterMax"></param>
        /// <param name="wRemoveMsg"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PeekMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        /// <summary>
        /// Unmanaged function for translating a message from the message queue from user32.dll.
        /// </summary>
        /// <param name="idThread"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern bool PostThreadMessage(int idThread, uint Msg, IntPtr wParam, IntPtr lParam);

    }
}
