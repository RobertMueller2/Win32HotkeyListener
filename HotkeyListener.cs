using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Interop;
using Win32HotkeyListener.Win32;
using static Win32HotkeyListener.Win32.User32MessageFunctions;

namespace Win32HotkeyListener {

    public class HotkeyListener {

        private readonly Logger logger;
        private BackgroundWorker worker;

        private ConcurrentDictionary<uint, (BaseHotkey, User32Hotkey)> FinalHotkeys = new ConcurrentDictionary<uint, (BaseHotkey, User32Hotkey)>();

        public IEnumerable<BaseHotkey> Hotkeys { get; set; }
        public bool Running { get; set; } = false;

        public delegate void HotkeyListenerCallback();

        public HotkeyListenerCallback OnCompleted { get; set; }


        public HotkeyListener(IEnumerable<BaseHotkey> hotkeys) {

            this.Hotkeys = hotkeys;
            this.logger = Logger.GetInstance();

        }

        private void RegisterHotkeys() {
            uint i = 0;
            uint e = 0;
            foreach (var h in Hotkeys) {

                var kc = h.KeyCombo;
                var uh = new User32Hotkey(i, kc.ModifierCombo, kc.Key.Keycode);

                if (!uh.Initialised) {
                    e++;
                    uh.Dispose();
                    continue;
                }

                if (FinalHotkeys.TryAdd(i, (h, uh))) {
                    i++;
                    h.Registered = true;
                }
                else {
                    e++; // this should never happen
                }
            }
            logger.Log(string.Format("Registered {0} hotkeys, {1} errors", i, e), MessageType.Info, PresentationType.ToolTip);
        }

        private void UnregisterHotkeys() {
            logger.Log("Unregistering hotkeys", MessageType.Debug, PresentationType.None);
            foreach (var h in FinalHotkeys) {
                h.Value.Item2.Dispose();
                FinalHotkeys.TryRemove(h.Key, out _);
            }
            logger.Log("Unregistered hotkeys", MessageType.Debug, PresentationType.None);
        }

        public void Run() {
            if (worker == null) {
                worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(Backgroundworker_DoWork);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Backgroundworker_RunWorkerCompleted);
                worker.WorkerSupportsCancellation = true;
            }
            worker.RunWorkerAsync();
            lock (this) {
                Running = true;
            }
        }

        public void Stop() {
            logger.Log("Cancelling hotkey listener worker", MessageType.Debug, PresentationType.None);
            worker.CancelAsync();
        }

        public void Backgroundworker_DoWork(object sender, DoWorkEventArgs e) {

            RegisterHotkeys();

            while (!worker.CancellationPending) {
                //TODO: consider filtering
                if (User32MessageFunctions.PeekMessageA(out MSG message, IntPtr.Zero, 0, 0, 0x0001)) {

                    if (message.message == (int)MsgType.WM_QUIT) {
                        logger.Log("Received WM_QUIT", MessageType.Debug, PresentationType.None);
                        //break;
                    }

                    //WM_HOTKEY: 786
                    if (message.message != (int)MsgType.WM_HOTKEY) {
                        continue;
                    }

                    logger.Log(string.Format("Message {0:X}, Hotkey: {1:X}", message.message, message.wParam.ToInt32()), MessageType.Debug, PresentationType.None);

                    var hotkey = FinalHotkeys[(uint)message.wParam.ToInt32()].Item1;

                    if (!hotkey.Enabled) {
                        logger.Log(string.Format("Hotkey disabled", hotkey.ToString()), MessageType.Debug, PresentationType.None);
                        continue;
                    }
                    var action = hotkey.Action;

                    logger.Log(string.Format("{0} | Executing {1}", hotkey.ToString(), action.ToString()), MessageType.Debug, PresentationType.None);

                    var ExecuteMethod = action.GetType().GetMethod("Execute");

                    try {
                        ExecuteMethod.Invoke(action, new Object[] { });
                    }
                    catch (Exception ex) {
                        logger.Log(string.Format("Error executing action: {0}", ex.Message), MessageType.Error, PresentationType.Popup);
                    }
                }
                else {
                    Thread.Sleep(10);
                }
            }

            UnregisterHotkeys();
        }

        public void Backgroundworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            lock (this) {
                Running = false;
            }
            logger.Log("Cancelled hotkey listener worker", MessageType.Debug, PresentationType.None);
            OnCompleted?.Invoke();
        }

        public void CheckBGWorker() {

            if (worker != null && (!Running || worker.IsBusy)) {
                return;
            }

            logger.Log("No Backgroundworker present, consider reloading", MessageType.Warning, PresentationType.ToolTip);
        }
    }

}