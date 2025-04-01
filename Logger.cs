using System.Collections.Concurrent;
using System.Text;
using System.IO;


namespace Win32HotkeyListener {

    /// <summary>
    /// Singleton class for logging. Can be used as drop in replacement for Console.WriteLine.
    /// 
    /// TODO: allow disabling of logging
    /// </summary>
    public class Logger : TextWriter {

        /// <summary>
        /// Lazy instantiation of the singleton instance.
        /// </summary>
        private static readonly Lazy<Logger> instance = new Lazy<Logger>(() => new Logger());

        /// <summary>
        /// Queue for storing log messages. The playback queue is used to store the messages that are returned by the Playback method,
        /// which removes the played back messages from the queue.
        /// 
        /// The main use case is to process new messages for presentation in a frontend, while allowing these to pile up when the frontend is busy or not running yet
        /// </summary>
        private ConcurrentQueue<LogMessage> logPlaybackQueue = new ConcurrentQueue<LogMessage>();

        /// <summary>
        /// Queue for storing log messages. This queue is used to store all log messages as capacity permits, it is not emptied.
        /// 
        /// This can be used to show a full log (up to capacity) at any time.
        /// </summary>
        private ConcurrentQueue<LogMessage> logBufferQueue = new ConcurrentQueue<LogMessage>();

        /// <summary>
        /// Private constructor for singleton, instantiation should be done via <see cref="GetInstance"/>.
        /// </summary>
        private Logger() { }

        /// <summary>
        /// Returns the singleton instance of the logger.
        /// </summary>
        /// <returns></returns>
        public static Logger GetInstance() {
            return instance.Value;
        }

        /// <summary>
        /// Implements the abstract Encoding property of TextWriter.
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Whether a fatal error has occurred. This is set to true when a fatal error is logged.
        /// </summary>
        public bool FatalError { get; private set; } = false;

        /// <summary>
        /// Whether to limit the capacity of the playback queue to <see cref="MaxQueueSize"/>.
        /// The default is false, the assumption is that the playback queue is regularly emptied by calling <see cref="Playback"/>.
        /// </summary>
        public bool EnforceMaxQueueSizeForPlaybackQueue { get; set; } = false;

        /// <summary>
        /// Whether to limit the capacity of the buffer queue to <see cref="MaxQueueSize"/>.
        /// </summary>
        public bool EnforceMaxQueueSizeForBufferQueue { get; set; } = true;

        /// <summary>
        /// Event that is fired when a new log message is added.
        /// </summary>
        public event Action<string>? LogUpdated;

        /// <summary>
        /// sets the maximum number of log messages to be stored in the queue.  If the queue is full, the oldest message will be removed.
        /// 
        /// TODO: if this is modified, check and dequeue items over capacity from logBufferQueue.
        /// </summary>
        public int MaxQueueSize { get; set; } = 1000;

        /// <summary>
        /// Convenience method to avoid code duplication, enqueues a log message in the given queue.
        /// Optionally respects the MaxQueueSize property and dequeues the oldest message if the queue is full.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="queue"></param>
        /// <param name="dequeue"></param>
        private void EnqueueLog(LogMessage msg, ConcurrentQueue<LogMessage> queue, bool dequeue = true) {
            queue.Enqueue(msg);
            if (!dequeue) {
                return;
            }
            while (queue.Count > MaxQueueSize) {
                queue.TryDequeue(out var _);
            }
        }

        /// <summary>
        /// Logs a message from the given message string with the given <see cref="MessageType"/> and <see cref="PresentationType"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="presentation"></param>
        public void Log(string message, MessageType type = MessageType.Info, PresentationType presentation = PresentationType.None) {

            if (type == MessageType.Fatal) {
                FatalError = true;
            }

            var msg = new LogMessage(message, type, presentation);
            EnqueueLog(msg, logBufferQueue, EnforceMaxQueueSizeForBufferQueue);
            EnqueueLog(msg, logPlaybackQueue, EnforceMaxQueueSizeForPlaybackQueue);

            LogUpdated?.Invoke(msg.ToString());
        }

        /// <summary>
        /// Returns the log messages in the queue as a tuple of lists of <see cref="LogMessage"/>s, grouped by <see cref="MessageType"/>.
        /// 
        /// Dequeues all messages from the queue.
        /// </summary>
        /// <returns></returns>
        public (List<LogMessage>, List<LogMessage>, List<LogMessage>) Playback() {
            var fatalMessages = new List<LogMessage>();
            var nonFatalMessages = new List<LogMessage>();
            var infoMessages = new List<LogMessage>();

            while (logPlaybackQueue.Count > 0) {
                logPlaybackQueue.TryDequeue(out var msg);
                if (msg != null) {
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
            }

            return (fatalMessages, nonFatalMessages, infoMessages);
        }


        /// <summary>
        /// Implements the abstract Write method of TextWriter.
        /// 
        /// TODO: consider providing a different signature that allows for specifying the <see cref="MessageType"/> and/or <see cref="PresentationType"/>.
        /// On the other hand, the <see cref="Log"/> method does that already.
        /// </summary>
        /// <param name="value"></param>
        public override void Write(string? value) {
            Log(value ?? "", MessageType.Debug);
        }

        /// <summary>
        /// Implements the abstract WriteLine method of TextWriter.
        /// </summary>
        /// <param name="value"></param>
        public override void WriteLine(string? value) {
            Log(value ?? "", MessageType.Debug);
        }

        /// <summary>
        /// Returns the log messages in the queue as an enumerable of <see cref="LogMessage"/>s.
        /// 
        /// TODO: allow log level selection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LogMessage> GetLogs() {
            return logBufferQueue.AsEnumerable();
        }

        /// <summary>
        /// Returns the log messages in the queue as an array of <see cref="LogMessage"/>s.
        /// 
        /// TODO: allow log level selection
        /// </summary>
        /// <returns></returns>
        public LogMessage[] GetLogArray() {
            return logBufferQueue.ToArray<LogMessage>();
        }
    }
}