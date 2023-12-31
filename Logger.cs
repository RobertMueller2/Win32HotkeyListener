using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Win32HotkeyListener {
    public class Logger : TextWriter {

        private static readonly Lazy<Logger> instance = new Lazy<Logger>(() => new Logger());

        private ConcurrentQueue<LogMessage> logPlaybackQueue = new ConcurrentQueue<LogMessage>();
        private ConcurrentQueue<LogMessage> logBufferQueue = new ConcurrentQueue<LogMessage>();

        private Logger() { }

        public static Logger GetInstance() {
            return instance.Value;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public bool FatalError { get; private set; } = false;

        public event Action<string> LogUpdated;

        /// <summary>
        /// sets the maximum number of log messages to be stored in the queue.  If the queue is full, the oldest message will be removed.
        /// </summary>
        public int MaxQueueSize { get; set; } = 1000;

        public void EnqueueLog(LogMessage msg, ConcurrentQueue<LogMessage> queue, bool dequeue = true) {
            queue.Enqueue(msg);
            if (!dequeue) {
                return;
            }
            while (queue.Count > MaxQueueSize) {
                queue.TryDequeue(out var _);
            }
        }

        public void Log(string message, MessageType type = MessageType.Info, PresentationType presentation = PresentationType.None) {

            if (type == MessageType.Fatal) {
                FatalError = true;
            }

            var msg = new LogMessage(message, type, presentation);
            EnqueueLog(msg, logBufferQueue, false);
            EnqueueLog(msg, logPlaybackQueue);

            LogUpdated?.Invoke(msg.ToString());
        }

        public (List<LogMessage>, List<LogMessage>, List<LogMessage>) Playback() {
            var fatalMessages = new List<LogMessage>();
            var nonFatalMessages = new List<LogMessage>();
            var infoMessages = new List<LogMessage>();

            while (logPlaybackQueue.Count > 0) {
                logPlaybackQueue.TryDequeue(out var msg);

                if (msg.Type == MessageType.Fatal) {
                    fatalMessages.Add(msg);
                }
                else if (msg.Type == MessageType.Error && (msg.Presentation == PresentationType.Popup || msg.Presentation == PresentationType.ToolTip)) {
                    nonFatalMessages.Add(msg);
                }
                else if (msg.Type == MessageType.Info) {
                    infoMessages.Add(msg);
                }
            }

            return (fatalMessages, nonFatalMessages, infoMessages);
        }

        //FIXME: this should do something meaningful
        //TODO: Do something clever about the log level
        public override void Write(string value) {
            Log(value, MessageType.Debug);
        }

        //TODO: Do something clever about the log level
        public override void WriteLine(string value) {
            Log(value, MessageType.Debug);
        }

        //TODO: Log level selection
        public IEnumerable<LogMessage> GetLogs() {
            return logBufferQueue.AsEnumerable();
        }

        public LogMessage[] GetLogArray() {
            return logBufferQueue.ToArray<LogMessage>();
        }
    }
}