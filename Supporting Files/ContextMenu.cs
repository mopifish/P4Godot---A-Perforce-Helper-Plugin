#if TOOLS
using System;
using Godot;
using Godot.Collections;

namespace RustedSnow.addons.P4GodotIntegration.Supporting_Files;

[Tool]
public partial class ContextMenu : EditorContextMenuPlugin {
    [Signal]
    public delegate void CheckoutFilePressedEventHandler(string[] files);
    
    [Signal]
    public delegate void RevertFilePressedEventHandler(string[] files);


    public PerforceClient _client = null;
    
    public override void _GetMenuOptions(Dictionary<StringName, Variant> options) {
        AddContextMenuItem("Checkout", new Callable(this, MethodName.OnCheckoutPressed), GD.Load<Texture2D>("res://addons/P4GodotIntegration/Assets/perforce_check.svg"));
        
        // Check if every file we have selected is opened. If one file is not open, do not allow revert.
        bool areFilesOpened = true;
        foreach (string file in options["selected_files"].AsStringArray())
        {
            if (!_client.IsFileOpened(ProjectSettings.GlobalizePath(file))) areFilesOpened = false;
        }

        if (areFilesOpened) {
            AddContextMenuItem("Revert", new Callable(this, MethodName.OnRevertPressed), GD.Load<Texture2D>("res://addons/P4GodotIntegration/Assets/perforce_revert.svg"));
        }
        
    }
    
    public void OnRevertPressed(Dictionary<StringName, Variant> options) {
        EmitSignalRevertFilePressed(options["selected_files"].AsStringArray());
    }
    
    public void OnCheckoutPressed(Dictionary<StringName, Variant> options) {
        EmitSignalCheckoutFilePressed(options["selected_files"].AsStringArray());
    }
}
#endif