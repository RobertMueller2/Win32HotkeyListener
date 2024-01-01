# Win32HotkeyListener

This is a library I'm using in my projects

- ErsatzToolbox
- WorkspaceSwitch.NET

which are not public yet. They had very similar hotkey code and I was tired to edit it in two places. And I might even need it in a third application.

The basic idea is to define a list of Hotkeys (extension of abstract class `BaseHotkey`) with key, key modifiers and an action, all of which can be de-/serialised from/to XML (but don't have to), then pass them to `HotkeyListener` which triggers registration and listens to the given hotkeys. Its action is executed when an enabled hotkey is pressed.

Includes a logger.

I do not consider the interface stable at this point, so I might just break everything in the future as I see fit for my apps that use this. But since it's on github anyway, I thought I could share it for reference purposes.

More explanations TBD.

Future ideas
--
- Right now, the hotkeys are global. If there's a use case for local hotkeys, it's probably easy to do.
- The logger can't be disabled but should. That said, it might be possible to set the `MaxQueueLimit` to 0 and both `Enforce` properties to `true`. I have not tried this.
- Some improvements here and there, consider the TODO remarks in the source
