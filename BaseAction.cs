using System;

namespace Win32HotkeyListener {
    public class BaseAction {

        /// <summary>
        /// executes the given action
        /// </summary>
        /// <returns>Whether the action was performed successfully.</returns>
        public virtual bool Execute() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Overridden ToString method for use in logging.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return this.GetType().Name;
        }

    }
}