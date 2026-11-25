#if TOOLS

using Godot;
using System;

namespace RustedSnow.addons.P4GodotIntegration.UI;

[Tool]
public partial class UiP4Tab : EditorDock
{
    [Signal]
    public delegate void CheckoutFilePressedEventHandler();
    
    [Signal]
    public delegate void RevertFilePressedEventHandler();
    
    [Signal]
    public delegate void CheckoutResourcePressedEventHandler();
    
    [Signal]
    public delegate void RevertResourcePressedEventHandler();
    
    [Signal]
    public delegate void CheckoutScriptPressedEventHandler();
    
    [Signal]
    public delegate void RevertScriptPressedEventHandler();

    
    public override void _Ready() {
        GetNode<Button>("%CheckoutSceneButton").Pressed += () => { EmitSignalCheckoutFilePressed(); };       
        GetNode<Button>("%RevertSceneButton").Pressed += () => { EmitSignalRevertFilePressed(); };       
        GetNode<Button>("%CheckoutResourceButton").Pressed += () => { EmitSignalCheckoutResourcePressed(); };       
        GetNode<Button>("%RevertResourceButton").Pressed += () => { EmitSignalRevertResourcePressed(); };       
        GetNode<Button>("%CheckoutScriptButton").Pressed += () => { EmitSignalCheckoutScriptPressed(); };       
        GetNode<Button>("%RevertScriptButton").Pressed += () => { EmitSignalRevertScriptPressed(); };       
    }

    public void SetSceneAvailable(bool value) {
        GetNode<Control>("%SceneContainer").Visible = value;
    }
    
    public void SetCheckedOut(bool value) {
        if (!IsNodeReady()) return;
        if (value) {
            GetNode<Control>("%SceneCheckedOut").Visible = true;
            GetNode<Control>("%SceneNotCheckedOut").Visible = false;
            GetNode<Button>("%RevertSceneButton").Visible = true;
        } else {
            GetNode<Control>("%SceneCheckedOut").Visible = false;
            GetNode<Control>("%SceneNotCheckedOut").Visible = true;
            GetNode<Button>("%RevertSceneButton").Visible = false;
        }
    }
    
    public void SetResourceAvailable(bool value)
    {
        GetNode<Control>("%ResourceContainer").Visible = value;
    }
    
    public void SetResourceCheckedOut(bool value) {
        if (!IsNodeReady()) return;
        if (value) {
            GetNode<Control>("%ResourceCheckedOut").Visible = true;
            GetNode<Control>("%ResourceNotCheckedOut").Visible = false;
            GetNode<Button>("%RevertResourceButton").Visible = true;
        } else {
            GetNode<Control>("%ResourceCheckedOut").Visible = false;
            GetNode<Control>("%ResourceNotCheckedOut").Visible = true;
            GetNode<Button>("%RevertResourceButton").Visible = false;
        }
    }
    
    public void SetScriptAvailable(bool value)
    {
        GetNode<Control>("%ScriptContainer").Visible = value;
    }
    
    public void SetScriptCheckedOut(bool value) {
        if (!IsNodeReady()) return;
        if (value) {
            GetNode<Control>("%ScriptCheckedOut").Visible = true;
            GetNode<Control>("%ScriptNotCheckedOut").Visible = false;
            GetNode<Button>("%RevertScriptButton").Visible = true;
        } else {
            GetNode<Control>("%ScriptCheckedOut").Visible = false;
            GetNode<Control>("%ScriptNotCheckedOut").Visible = true;
            GetNode<Button>("%RevertScriptButton").Visible = false;
        }
    }
    
}
#endif