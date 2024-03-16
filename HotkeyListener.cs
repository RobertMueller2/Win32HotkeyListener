using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Interop;
using Win32HotkeyListener.Win32;
using static Win32HotkeyListener.Win32.User32MessageFunctions;

namespace Win32HotkeyListener {

    /// <summary>
    /// HotkeyListener based on a BackgroundWorker.
    /// </summary>
    public class HotkeyListener {

        private readonly Logger logger;
        private BackgroundWorker worker;

        private ConcurrentDictionary<uint, BaseHotkey> IdToHotkey = new ConcurrentDictionary<uint, BaseHotkey>();

        /// <summary>
        /// Hotkeys to listen for, these need to be passed from the outside.
        /// </summary>
        public IEnumerable<BaseHotkey> Hotkeys { get; set; }

        private bool _running = false;
        /// <summary>
        /// Whether the listener is running or not.
        /// </summary>
        public bool Running { 
            get => _running;
            set {
                _running = value;
                RunningChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event to be executed when the Running property changes.
        /// </summary>
        public event EventHandler RunningChanged;

        /// <summary>
        /// Delegate definition for the callback to be executed when the BackgroundWorker is done.
        /// </summary>
        public delegate void HotkeyListenerCallback();

        /// <summary>
        /// Callback to be executed when the BackgroundWorker is done.
        /// </summary>
        public HotkeyListenerCallback OnCompleted { get; set; }

        /// <summary>
        /// Constructor for HotkeyListener, takes a list of hotkeys to listen for.
        /// </summary>
        /// <param name="hotkeys"></param>
        public HotkeyListener(IEnumerable<BaseHotkey> hotkeys) {

            this.Hotkeys = hotkeys;
            this.logger = Logger.GetInstance();

        }

        /// <summary>
        /// Register all hotkeys with the OS.
        /// </summary>
        private void RegisterHotkeys() {
            uint i = 0;
            uint e = 0;

            foreach (var h in Hotkeys) {
                if (h.TryRegister(i) && IdToHotkey.TryAdd(i, h)) {
                    i++;
                } else {
                    // there is no specific indication what exactly failed, but TryAdd failure is very unlikely
                    e++;
                }
            }

            logger.Log(string.Format("Registered {0} hotkeys, {1} errors", i, e), MessageType.Info, PresentationType.ToolTip);
        }

        /// <summary>
        /// Unregister all hotkeys with the OS.
        /// </summary>
        private void UnregisterHotkeys() {
            logger.Log("Unregistering hotkeys", MessageType.Debug, PresentationType.None);
            foreach (var h in IdToHotkey) {
                h.Value.Unregister();
                IdToHotkey.TryRemove(h.Key, out _);
            }
            logger.Log("Unregistered hotkeys", MessageType.Debug, PresentationType.None);
        }

        /// <summary>
        /// Force unregister all hotkeys with the OS. This includes hotkeys that were not successfully registered.
        /// </summary>
        public void ForceUnregisterHotkeys() {
            if (Running) {
                logger.Log("Force unregistering hotkeys requested but hotkey listener is still running", MessageType.Debug, PresentationType.None);
                return;
            }
            logger.Log("Force unregistering hotkeys", MessageType.Debug, PresentationType.None);
            foreach (var h in Hotkeys) {
                h.Unregister();
            }
            logger.Log("Force unregistered hotkeys", MessageType.Debug, PresentationType.None);
        }

        /// <summary>
        /// Start the BackgroundWorker.
        /// </summary>
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

        /// <summary>
        /// Stop the BackgroundWorker.
        /// </summary>
        public void Stop() {
            logger.Log("Cancelling hotkey listener worker", MessageType.Debug, PresentationType.None);
            worker.CancelAsync();
        }

        /// <summary>
        /// Worker method for the BackgroundWorker.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    var hotkey = IdToHotkey[(uint)message.wParam.ToInt32()];
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

        /// <summary>
        /// Method to be executed when the BackgroundWorker is done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Backgroundworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            lock (this) {
                Running = false;
            }
            logger.Log("Cancelled hotkey listener worker", MessageType.Debug, PresentationType.None);
            OnCompleted?.Invoke();
        }

        /// <summary>
        /// Check whether the BackgroundWorker is running and log a warning if not.
        /// </summary>
        public void CheckBGWorker() {

            if (worker != null && (!Running || worker.IsBusy)) {
                return;
            }

            logger.Log("No Backgroundworker present, consider reloading", MessageType.Warning, PresentationType.ToolTip);
        }
    }

}
