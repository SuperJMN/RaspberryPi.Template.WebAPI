#addin "Cake.Putty"

// REPLACE WITH YOUR VALUES _________________________________________________________

var appFolder=@"/home/pi/DotNet/RaspbianWebApi";		
var piHostname="raspberrypi";
var piUser = "pi";

// __________________________________________________________________________________



var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "netcoreapp2.0");

///////////////////////////////////////////////////////////////////////
// ARGUMENTS (WITH DEFAULT PARAMETERS FOR LINUX (Ubuntu 16.04, Raspbian Jessie, etc)
///////////////////////////////////////////////////////////////////////
var runtime = Argument("runtime", "linux-arm");
var destinationIp = Argument("destinationPi", piHostname);
var destinationDirectory = Argument("destinationDirectory", appFolder);
var username = Argument("username", piUser);
var sessionname = Argument("sessionname", "<<YOUR SAVED PUTTY SESSION NAME>");
var executableName = Argument("executableName", "RaspbianWebApi");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var binaryDir = Directory("./bin");
var objectDir = Directory("./obj");
var publishDir = Directory("./publish");
var projectFile = "./" + executableName + ".sln";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(binaryDir);
        CleanDirectory(objectDir);
        CleanDirectory(publishDir);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore(projectFile);
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Framework = framework,
            Configuration = configuration,
            OutputDirectory = "./bin/"
        };

        DotNetCoreBuild(projectFile, settings);
    });

Task("Publish")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var settings = new DotNetCorePublishSettings
        {
            Framework = framework,
            Configuration = configuration,
            OutputDirectory = "./publish/",
            Runtime = runtime
        };
                    
        DotNetCorePublish(projectFile, settings);
    });

Task("Deploy")
    .IsDependentOn("Publish")
    .Does(() =>
    {
        var files = GetFiles("./publish/*");
        
        if(runtime.StartsWith("win")) 
        {
            var destination = @"\\" + destinationIp + @"\" + destinationDirectory;
            CopyFiles(files, destination, true);
        }
        else
        {
            var createDirCommand = "mkdir -p " + destinationDirectory;

            StartProcess("plink", new ProcessSettings {
            		Arguments = new ProcessArgumentBuilder()
                	.Append(@"-load " + sessionname)
			.Append("-l " + username)
                	.Append(createDirCommand)
            	}
            );

            var destination = destinationIp + ":" + destinationDirectory;
            var fileArray = files.Select(m => @"""" + m.ToString() + @"""").ToArray();
            Pscp(fileArray, destination, new PscpSettings
                                                { 
                                                    SshVersion = SshVersion.V2, 
                                                    User = username 
                                                }
            );

            var plinkCommand = "chmod u+x,o+x " + destinationDirectory + "/" + executableName;
            Plink(username + "@" + destination, plinkCommand);
        }
    });

Task("DeployWithPuTTYSession")
    .IsDependentOn("Publish")
    .Does(() =>
    {
        var files = GetFiles("./publish/*");
        
            var createDirCommand = "mkdir -p " + destinationDirectory;

            StartProcess("plink", new ProcessSettings {
            		Arguments = new ProcessArgumentBuilder()
                	.Append(@"-load " + sessionname)
			.Append("-l " + username)
                	.Append(createDirCommand)
            	}
            );

        var destination = sessionname + ":" + destinationDirectory;
        var fileArray = files.Select(m => @"""" + m.ToString() + @"""").ToArray();
        Pscp(fileArray, destination, new PscpSettings
                                            { 
                                                SshVersion = SshVersion.V2 
                                            }
        );

        var plinkCommand = "chmod u+x,o+x " + destinationDirectory + "/" + executableName;

        StartProcess("plink", new ProcessSettings {
            Arguments = new ProcessArgumentBuilder()
                .Append(@"-load")
                .Append(sessionname)
                .Append(plinkCommand)
            }
        );
    });

Task("Default")
    .IsDependentOn("Deploy");

RunTarget(target);