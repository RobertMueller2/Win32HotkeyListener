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

        public override string ToString() {
            return this.GetType().Name;
        }


    }
}