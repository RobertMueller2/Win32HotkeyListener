using System;

namespace Win32HotkeyListener {

    public enum MessageType {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public enum PresentationType {
        Popup,
        ToolTip,
        None
    }

    public class LogMessage {

        public string Message { get; set; }

        public MessageType Type { get; set; }

        public PresentationType Presentation { get; set; }

        public DateTime Timestamp = DateTime.Now;

        public LogMessage(string message, MessageType type = MessageType.Info, PresentationType presentation = PresentationType.None) {
            Message = message;
            Type = type;
            Presentation = presentation;
        }

        //TODO: timestamp formatting
        public override string ToString() {
            return $"[{Timestamp.ToString()}] [{Type.ToString()}] {Message}";
        }
    }
}
