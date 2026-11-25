/*
This file is responsible for initially setting up the Perforce Integration
*/

#if TOOLS
using System.Collections.Generic;
using Godot;
using RustedSnow.addons.P4GodotIntegration.Supporting_Files;
using RustedSnow.addons.P4GodotIntegration.UI;

namespace RustedSnow.addons.P4GodotIntegration;

[Tool]
public partial class P4GodotIntegration_Launcher : EditorPlugin
{
	
	private PerforceClient _client;
	private Node _currentSceneRoot = null;
	private SceneWatcher _sceneWatcher = null; 
	
	private UiP4Tab _dock;
	private ContextMenu _contextMenu;

	private HashSet<string> _dirtyResources;
	
	public override void _EnterTree()
	{
		_client = new PerforceClient();
		_client.FileCacheUpdated += RefreshDock;
		
		_sceneWatcher = new SceneWatcher();
		_sceneWatcher.PreSave += OnScenePresave;
		
		ResourceSaved += OnResourceSaved;
		SceneSaved += OnSceneSaved;
		
		SceneChanged += OnSceneChanged;
		
		_dock = ResourceLoader.Load<PackedScene>("res://addons/P4GodotIntegration/UI/UI_P4Tab.tscn").Instantiate<UiP4Tab>();
		_dock.CheckoutFilePressed += OnCheckoutCurrentScenePressed;
		_dock.RevertFilePressed += OnRevertCurrentScenesPressed;
		_dock.CheckoutResourcePressed += OnCheckoutCurrentResourcePressed;
		_dock.RevertResourcePressed += OnRevertCurrentResourcePressed;
		_dock.CheckoutScriptPressed += OnCheckoutCurrentScriptPressed;
		_dock.RevertScriptPressed += OnRevertCurrentScriptPressed;
		AddDock(_dock);

		EditorInterface.Singleton.GetInspector().EditedObjectChanged += () =>
		{
			GodotObject editedObject = EditorInterface.Singleton.GetInspector().GetEditedObject();
			if (editedObject is Resource resource)
			{
				string resourcePath = ProjectSettings.GlobalizePath(resource.GetPath());
				_dock.SetResourceAvailable(true);
				_dock.SetResourceCheckedOut(_client.IsFileOpened(resourcePath));
			}
			else
			{
				_dock.SetResourceAvailable(false);
			}
		};
		
		EditorInterface.Singleton.GetInspector().PropertyEdited += property =>
		{
			GodotObject editedObject = EditorInterface.Singleton.GetInspector().GetEditedObject();
			if (editedObject is Resource resource)
			{
				OnCheckoutCurrentResourcePressed();
			}

			// if (editedObject is Node node)
			// {
			// 	GD.Print(node.GetTree().Root.SceneFilePath);
			// 	GD.Print(node.Owner?.SceneFilePath);
			// }
			
		};

		MainScreenChanged += name =>
		{
			_dock.SetScriptAvailable(name == "Script");
		};
		EditorInterface.Singleton.GetScriptEditor().EditorScriptChanged += script =>
		{
			RefreshDock();
		};
		
		_contextMenu = new ContextMenu();
		_contextMenu._client = _client;
		_contextMenu.CheckoutFilePressed += OnCheckoutFilesPressed;
		_contextMenu.RevertFilePressed += OnRevertFilesPressed;
		AddContextMenuPlugin(EditorContextMenuPlugin.ContextMenuSlot.Filesystem, _contextMenu);
	}

	public override void _Notification(int what)
	{
		// Update cache whenever we re-enter godot editor in case the user did some p4v operations.
		switch ((long)what)
		{
			case MainLoop.NotificationApplicationFocusIn:
				_client.UpdateCheckedoutFileCache();
				_dock.SetCheckedOut(_client.IsFileOpened(ProjectSettings.GlobalizePath(GetTree().EditedSceneRoot.SceneFilePath)));
				break;
		}
	}

	public override void _ExitTree()
	{
		_client.DisconnectFromPerforce();
		if (_dock != null) {
			RemoveDock(_dock);
			_dock.QueueFree();
		}
		
		RemoveContextMenuPlugin(_contextMenu);
	}

	void OnResourceSaved(Resource r)
	{
		//GD.Print("Resource Saved! ", r.ResourceName);
		// string fullPath = ProjectSettings.GlobalizePath(r.ResourcePath);
		// _client.CheckOutFile(fullPath);
	}

	void OnSceneSaved(string file)
	{
		// string fullPath = ProjectSettings.GlobalizePath(file);
		// _client.CheckOutFile(fullPath);
	}

	void OnSceneChanged(Node root) {
		if (_currentSceneRoot != null) {
			_currentSceneRoot.RemoveChild(_sceneWatcher);
		}
		if (root != null) {
			root.AddChild(_sceneWatcher);
		}
		// Ensure owner is null to prevent serializatixon
		_sceneWatcher.Owner = null;
		_currentSceneRoot = root;

		if (root == null) return;
		
		bool isFileChecked = _client.IsFileOpened(ProjectSettings.GlobalizePath(GetTree().EditedSceneRoot.SceneFilePath));
		_dock.SetCheckedOut(isFileChecked);
	}

	void OnScenePresave()
	{
		OnCheckoutCurrentScenePressed();
		OnCheckoutCurrentScriptPressed();
		OnCheckoutCurrentResourcePressed();
	}
	
	// == UI Delegates
	void RefreshDock()
	{
		// Edit scene widget
		if (GetTree().EditedSceneRoot != null) {
			_dock.SetSceneAvailable(true);
			string currentScene = ProjectSettings.GlobalizePath(GetTree().EditedSceneRoot.SceneFilePath);
			_dock.SetCheckedOut(_client.IsFileOpened(currentScene));
		} else {
			_dock.SetSceneAvailable(false);
		}
		
		
		
		// Edit resource widget
		GodotObject editedObject = EditorInterface.Singleton.GetInspector().GetEditedObject();
		if (editedObject is Resource resource)
		{
			_dock.SetResourceAvailable(true);
			string currentResource = ProjectSettings.GlobalizePath(resource.ResourcePath);
			_dock.SetResourceCheckedOut(_client.IsFileOpened(currentResource));			
		}
		else
		{
			_dock.SetResourceAvailable(false);
		}
		
		// Edit Script WIdget
		string currentScript = ProjectSettings.GlobalizePath(EditorInterface.Singleton?.GetScriptEditor()?.GetCurrentScript()?.ResourcePath);
		_dock.SetScriptCheckedOut(_client.IsFileOpened(currentScript));
		_dock.SetScriptAvailable(EditorInterface.Singleton.GetScriptEditor().IsVisibleInTree());
		
	}
	
	void OnCheckoutCurrentScenePressed()
	{
		string currentScene = ProjectSettings.GlobalizePath(GetTree().EditedSceneRoot.SceneFilePath);
	    if (!_client.IsFileOpened(currentScene))
	    {
			_client.CheckOutFile(currentScene);
	    }
	}
	
	void OnCheckoutFilesPressed(string[] files)
	{
		foreach (string file in files)
		{
			_client.CheckOutFile(ProjectSettings.GlobalizePath(file));
		}
	}
	
	void OnRevertCurrentScenesPressed()
	{
		_client.RevertFile(ProjectSettings.GlobalizePath(GetTree().EditedSceneRoot.SceneFilePath));
	}
	
	void OnRevertFilesPressed(string[] files)
	{
		foreach (string file in files)
		{
			_client.RevertFile(ProjectSettings.GlobalizePath(file));

		}
	}
	
	void OnCheckoutCurrentResourcePressed()
	{
		GodotObject editedObject = EditorInterface.Singleton.GetInspector().GetEditedObject();
		if (editedObject is Resource resource)
		{
			string resourcePath = ProjectSettings.GlobalizePath(resource.GetPath());
			if (!_client.IsFileOpened(resourcePath))
			{
				_client.CheckOutFile(resourcePath);
			}
		}
	}
	
	void OnRevertCurrentResourcePressed()
	{
		GodotObject editedObject = EditorInterface.Singleton.GetInspector().GetEditedObject();
		if (editedObject is Resource resource)
		{
			string resourcePath = ProjectSettings.GlobalizePath(resource.GetPath());
			if (_client.IsFileOpened(resourcePath))
			{
				_client.RevertFile(resourcePath);
			}
		}
	}
	
	void OnCheckoutCurrentScriptPressed()
		{
			string currentScript = ProjectSettings.GlobalizePath(EditorInterface.Singleton.GetScriptEditor().GetCurrentScript().ResourcePath);
			if (!_client.IsFileOpened(currentScript))
			{
				_client.CheckOutFile(currentScript);
			}
		}
		
		void OnRevertCurrentScriptPressed()
		{
			string currentScript = ProjectSettings.GlobalizePath(EditorInterface.Singleton.GetScriptEditor().GetCurrentScript().ResourcePath);
			if (_client.IsFileOpened(currentScript))
			{
				_client.RevertFile(currentScript);
			}
		}
}

#endif
