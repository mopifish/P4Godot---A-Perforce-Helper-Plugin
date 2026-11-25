/*
This file is responsible for connecting to the perforce server and communicating with it
*/

#if TOOLS
using System;
using System.Collections.Generic;
using Godot; 
using Perforce.P4;

namespace RustedSnow.addons.P4GodotIntegration.Supporting_Files;

// Sr
// struct PerforceData
// {
//     
// }

[Tool]
public partial class PerforceClient : EditorPlugin
{
    // The username of our P4 user
    private string _username;
    // The server and port to connect to in format "server:port"
    private string _serverName;
    // The name of the workspace to connect to
    private string _workspace;
    // The name of the specific stream we're in
    private string _stream;
    // The name of the specific depot we're in
    private string _depot;
    
    // The server we're connected to
    private Server _server;
    // The repository we're connected to
    private Repository _repository;
    // Our actual connection to said server + repository
    private Connection _connection;

    private IList<File> _checkedoutFileCache;
    
    [Signal]
    public delegate void FileCacheUpdatedEventHandler();
    
    
    public PerforceClient()
    {
        // GD.Print("Making a new perforce client");
        Initialize();
    }

    // Sets up the connection between the Perforce Client and the server
    private void Initialize()
    {
        // Would be nice to figure out a way to do this without command line but idk enough about the P4V API to do that

        // Get the information of the current user
        Godot.Collections.Array information = [];
        int result = OS.Execute("p4", ["info"], information, true);

        // On success, execute returns 0
        if (result == 0)
        {

            // Optimization idea: Look at the specific rows inside of the resulting array instead of parsing all of them
            // Performance really shouldn't matter since we only do this once, but if there's ever a need...

            // All the information is dumped into the first element of the array, so we need to
            // get it out and convert it to a more useful format
            string perforceInfo = information[0].AsString();

            // Move each line into its own spot in an array
            // string[] eachLine = perforceInfo.Split("\n");



            // Split the text up per line and save the username, server/port, and workspace
            foreach (string line in perforceInfo.Split("\n"))
            {
                // GD.Print(line);
                if (line.StartsWith("User name"))
                {
                    int colonPos = line.Find(":");
                    _username = line.Substring(colonPos + 1).StripEdges();
                }

                else if (line.StartsWith("Server address"))
                {
                    int colonPos = line.Find(":");
                    // We are pre-pending ssl to our server name!!!!!!!!!!!!!!!!!!!!!!!!!
                    _serverName = "ssl:" + line.Substring(colonPos + 1).StripEdges();
                }

                else if (line.StartsWith("Client name"))
                {
                    int colonPos = line.Find(":");
                    _workspace = line.Substring(colonPos + 1).StripEdges();
                }
                else if (line.StartsWith("Client stream"))
                {
                    int colonPos = line.Find(":");
                    _stream = line.Substring(colonPos + 1).StripEdges();
                    
                    
                    int slashPos = line.RFind("/");
                    _depot = line.Substring(colonPos + 1, slashPos - colonPos).StripEdges();   
                    
                } 
                    
            }

           
        }

        // define the server, repository and connection
        _server = new Server(new ServerAddress(_serverName));
        _repository = new Repository(_server);
        _connection = _repository.Connection;

        // use the connection variables for this connection
        _connection.UserName = _username;
        _connection.Client = new Client { Name = _workspace };

        // connect to the server
        _connection.Connect(null);

        // get server metadata and check version
        // (using null for options parameter)
        ServerMetaData p4Info = _repository.GetServerMetaData(null);

        GD.Print("==========================================================");
        GD.Print("Connected to Perforce Server");
        GD.Print("Depot Name:", _depot);
        GD.Print("Username: " + _username + 
                 "\nWorkspace: " + _workspace +
                 "\nServer: " + _serverName + 
                 "\nStream: " + _stream
                 );
        //GD.Print("Server address : " + p4Info.Address);
        //GD.Print("Server root    : " + p4Info.Root);
        //GD.Print("Server version : " + p4Info.Version);
        //GD.Print("Server date    : " + p4Info.Date);
        GD.Print("==========================================================");

        _checkedoutFileCache = GetOpenedFiles();
    }

    public void UpdateCheckedoutFileCache()
    {
        _checkedoutFileCache = GetOpenedFiles();
        EmitSignalFileCacheUpdated();
    }
    
    // Currently unused because I don't know how to submit the default changelist
    void SubmitChangelist()
    {
        GD.Print("Attempting to submit changelist");

        // var changelistDescriptor = new DescribeChangelistCmdFlags(DescribeChangelistCmdFlags., 0, 0);
        
        // Grab the default changelist
        Changelist defaultChangelist = _repository.GetChangelist(0, null);

        defaultChangelist.Description = "Updated the perforce plugin to be able to check out files and submit them! (This submission is done through the plugin, so if you're reading this than it worked)";
        
        var submissionResult = defaultChangelist.Submit(null);
        
        GD.Print("Submitted changelist");
        
        UpdateCheckedoutFileCache();
    }

    // Checks out a file to the default changelist
    public bool CheckOutFile(String pathToFile)
    {
        if (IsFileOpened(pathToFile))
        {
            GD.Print("[P4] Failed to checkout file, file was already checked out.");
            return true;
        }
        
        // A filespec is basically just a versioned item inside Perforce
        FileSpec[] fileSpec = new FileSpec[] {
            new FileSpec( new ClientPath(pathToFile))
        };
        
        // Create default options
        Perforce.P4.Options options = new Options();
        
        // Just ALWAYS add a file to prevent errors. No good way to detect on add.
        var addedFiles = _connection.Client.AddFiles(options, fileSpec);
        if (addedFiles != null) {
            GD.Print("[P4] Added file ", pathToFile);
            UpdateCheckedoutFileCache();
            return true;
        } else {
        // If the file was added, it's already checked out. If it wasn't, attempt to check it out.
            
            // Actual command that lets us check out a file for editing
            var editedFiles = _connection.Client.EditFiles(options, fileSpec);
            
            if (editedFiles == null)  {
                GD.PrintErr("[P4 ERROR] Failed to checkout file  ", pathToFile);
                return false;
            } else {
                GD.Print("[P4] Checked out file ", pathToFile);
                UpdateCheckedoutFileCache();
                
                return true;
            }
        }
    }

    public bool IsFileOpened(String pathToFile) {
        IList<File> files = _checkedoutFileCache;
        
        if (files == null) 
        {
            // GD.PrintErr("[P4 ERROR] Failed to retrieve opened files from the repository. File validation will not function.");
            return false;
        }

        int rootFolderPos = pathToFile.Find(_workspace);
        if (rootFolderPos == -1) {
            GD.PrintErr($"[P4 ERROR] Failed to find workspace '{_workspace}' in filepath '{pathToFile}'");
            return false;
        }
        String filePath = pathToFile.Substring(rootFolderPos).StripEdges();
        
        foreach (File f in files) {
            String localPath = f.ClientPath.ToString();
            rootFolderPos = localPath.Find(_workspace);
            if (rootFolderPos == -1) continue; // This indicates the file is open in a different workspace from this user.
            
            localPath = localPath.Substring(rootFolderPos).StripEdges();
            
            if (localPath == filePath) {
                return true;
            }
        }
        
        return false;
    }
    
    public IList<File> GetOpenedFiles() {
        var fileSpecs = new List<FileSpec> 
        { 
            new FileSpec(new DepotPath(_depot + "..."), null) 
        };
        
        var options = new GetOpenedFilesOptions(
            GetOpenedFilesCmdFlags.AllClients,
            changelist: null,                 
            client: null,                      
            user: _username,                       
            maxItems: 999                        
        );
        
        IList<File> files = _repository.GetOpenedFiles(fileSpecs, options);
        return files;
    }

    // Reverts a file
    public bool RevertFile(string pathToFile)
    {
        if (!IsFileOpened(pathToFile))
        {
            GD.Print("[P4] Failed to revert file, file was not checked out.");
            return true;
        }
        
        // A filespec is basically just a versioned item inside Perforce
        FileSpec file = new FileSpec
        (
            new DepotPath(pathToFile)
        );
        var editedFiles = _connection.Client.RevertFiles(null, file);
        
        
        if (editedFiles == null)
        {
            GD.PrintErr("[P4 ERROR] Failed to revert file  ", pathToFile);
            return false;
        } else {
            GD.Print("[P4] Reverted file ", pathToFile);
            UpdateCheckedoutFileCache();
            
            return true;
        }
        
    }

    // Disconnects the plugin from the server
    // I don't actually know if this is necessary to do (I wasn't doing it originally and there weren't any errors),
    // But better safe than sorry
    public void DisconnectFromPerforce()
    {
        _connection.Disconnect();
    }
    
    // Ori's evil function to see what information we can gather from the DigiPen perforce server
    void TestingStuff()
    {
        ClientsCmdOptions flags = new ClientsCmdOptions
        (
            ClientsCmdFlags.None,
            "ori.balashov",
            "*rustedSnow",
            5,
            null
        );
        var clients = _repository.GetClients(flags);


        Client ori = clients[0];

        var depotPath = ori.Stream;

        foreach (var client in clients)
        {
            GD.Print(client.Name);
            GD.Print(client.Root);
            GD.Print(client.Stream);

        }

        GetDepotFilesCmdOptions opts =
            new GetDepotFilesCmdOptions(GetDepotFilesCmdFlags.None, 10);
        FileSpec fs = new FileSpec(new DepotPath("//depot/..."), null);
        IList<FileSpec> list = new List<FileSpec>();
        list.Add(fs);
        var files = _repository.GetDepotFiles(list, opts);

        foreach (var file in files)
        {
            GD.Print(file.DepotPath);
        }

        var depots = _repository.GetDepots(null);
 
        foreach (var depot in depots)
        {
            GD.Print(depot.Id);
        }
    }
}

#endif