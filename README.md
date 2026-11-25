# P4Godot
P4Godot is a Godot addon designed to improve Perforce integration. It adds a dock to the bottom right corner of your editor that shows whether the current Scene/Script/Resource is checked out.
It supports automatically checking out and adding files upon save/edit.

This plugin is intended to be used alongside P4V and reduce friction in Godot.

# Setup
### Workspace
Ensure your workspace's root directory matches your workspace's name!! The plugin uses the name of your workspace to localize filepaths. If this is not the case, you will get errors and the plugin will fail to work.

### Environment Info
This plugin relies on the environment info stored by P4V to initialize correctly, and is intended to be used alongside P4V.
To set up your environment info:
- Open P4V 
- Connection > Environment Settings
- Ensure that the "Use Current Connection for Environment Settings" is checked
- Click OK
- Restart Godot

### Additional Suggestions
Here are some additional suggested steps you can take to make P4 work better with godot:
- Mark your project file as 'always write' in p4v.
- Update your typemap to set '.import' files to always write

# Current Features
<img width="494" height="264" alt="image" src="https://github.com/user-attachments/assets/65036c5f-08e8-4f85-bb0b-abb6b42ca910" />

- UI Dock that shows whether the current scene, script, and resource are checked out or not
- Buttons to both check out and revert the current scene/script/resource
<img width="634" height="166" alt="image" src="https://github.com/user-attachments/assets/9cc54fd0-45a5-4e9f-a881-97463107efd8" />

- Context menu buttons to check out and revert files from the filesystem
- Automatic checking out of scenes and scripts pre-save
- Automatic checking out of resources on edit
- Automatic addition of scenes/resources/scripts on pre-save

# Known Issues
- Godot may try to save resources that have not been edited. This will throw an error and may create a temp file, but shouldn't cause any issues. 

# Contributing
This plugin was made as a tool for my game team, but unfortunately I'm just one student and can't put a lot of time into improving this plugin.
Please submit Issues here if you encounter bugs or have requests, or make Pull Requests for potential contributions. 
I will do my best to maintain and update this plugin as people push contributions, and would appreciate help in improving this plugin for everyone :)

# About
This plugin was made by students at the Digipen Institute of Technology as an assistant tool for their Junior Game Project.
The initial developers of this tool were Ori Balashov and Drake Pickett.
