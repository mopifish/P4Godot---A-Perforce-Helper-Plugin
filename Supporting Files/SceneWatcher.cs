#if TOOLS
namespace RustedSnow.addons.P4GodotIntegration.Supporting_Files;

using Godot;

// A helper class that monitors a scene and emits a signal when a scene is saved
public partial class SceneWatcher : Node {
    [Signal]
    public delegate void PreSaveEventHandler();
    
    public override void _Notification(int what) {
        switch ((long)what) {
            case NotificationEditorPreSave:
                EmitSignalPreSave();
                break;
        }
    }
}
#endif