using System;
using System.Collections.Generic;
using System.IO;

namespace tiled2Asset
{
    public class Configuration
    {
        public static List<string> headers = new List<string>();
        public static Dictionary<string, string> gbdkObjectDefaultProperties = new Dictionary<string, string>();
        public static Dictionary<string, string> gbdkObjectDefaultPropertyTypes = new Dictionary<string, string>();
        public static Dictionary<string, string> gbdkMapDefaultProperties = new Dictionary<string, string>();
        public static Dictionary<string, string> gbdkMapDefaultPropertyTypes = new Dictionary<string, string>();

        public static string gbdkObjectStructName = "TiledGBDKObject";
        public static string gbdkMapStructName = "TiledGBDKMap";

        public static string gbdkInstallationPath = null;
        public static string tiledInstallationPath = null;
        private static string sourcePath = "";
        private static string headersPath = "";

        public static bool generateObjectStruct = false;
        public static bool generateMapStruct = false;
        public static bool rasterizeTMXFiles = false;
        public static bool exportObjectStrings = false;
        public static bool generateStringLookupFunction = false;

        public static void ReadFromArgs(string[] args)
        {
            // Loop through our arguments
            for (int i = 0; i < args.Length; i++)
            {
                // Defines a property that all maps will have
                // If not overriden from tiled, it will give each map the default value specified
                if (args[i] == "--map-property")
                {
                    string type = args[i + 1];
                    string identifier = args[i + 2];
                    string defaultValue = args[i + 3];

                    gbdkMapDefaultProperties.Add(identifier,defaultValue);
                    gbdkMapDefaultPropertyTypes.Add(identifier,type);
                    Console.WriteLine("Map Property: " + identifier + " is a " +type+ " which defaults to " +gbdkMapDefaultProperties);

                    i += 3;
                }

                // Defines a property that all objects will have
                // If not overriden from tiled, it will give each object the default value specified
                else if (args[i] == "--object-property")
                {
                    string type = args[i + 1];
                    string identifier = args[i + 2];
                    string defaultValue = args[i + 3];

                    if (!gbdkObjectDefaultProperties.ContainsKey(identifier))
                    {

                        gbdkObjectDefaultProperties.Add(identifier,defaultValue);
                        gbdkObjectDefaultPropertyTypes.Add(identifier, type);
                        Console.WriteLine("Object Property: " + identifier + " is a " +type+ " which defaults to " + defaultValue);
                    }
                    else
                    {
                        Console.WriteLine("Duplicate Property Specified: " + identifier + ". Using first.");
                    }


                    i += 3;
                }

                // Can be done multiple times
                // This will add headers to include on the map c files
                else if (args[i] == "--gbdk-header")
                {
                    headers.Add(args[i + 1]);
                    i++;
                }

                // The name of the struct holding map data
                // Alternativel specifiy --generate-map-struct
                else if (args[i] == "--map-struct-name")
                {
                    gbdkMapStructName = args[i + 1];
                    i++;
                }

                // The name of the struct holding object data
                // Alternativel specifiy --generate-object-struct
                else if (args[i] == "--object-struct-name")
                {
                    gbdkObjectStructName = args[i + 1];
                    i++;
                }

                // Where the tiled.exe and tmxrasterizer.exe are installed
                else if (args[i] == "--tiled-installation-path")
                {
                    tiledInstallationPath = args[i + 1];
                    i++;
                }

                // The GBDK 2020  base folder
                else if (args[i] == "--gbdk-installation-path")
                {
                    gbdkInstallationPath = args[i + 1];
                    i++;
                }

                // Should an automatic struct be generated for objects
                else if (args[i] == "--generate-object-struct")
                {
                    generateObjectStruct=true;
                }

                // Should an automatic struct be generated for maps
                else if (args[i] == "--generate-map-struct")
                {
                    generateMapStruct = true;
                }

                // Should an automatic struct be generated for maps
                else if (args[i] == "--export-strings")
                {
                    exportObjectStrings = true;
                }

                // Where to create source
                else if (args[i] == "--source-out-path")
                {
                    sourcePath = args[i + 1];

                    // Create the directories if they don't exist
                    if (!Directory.Exists(sourcePath)) Directory.CreateDirectory(sourcePath);
                    i++;
                }

                // Where to create headers
                else if (args[i] == "--header-out-path")
                {
                    headersPath = args[i + 1];

                    // Create the directories if they don't exist
                    if (!Directory.Exists(headersPath))Directory.CreateDirectory(headersPath);
                    i++;
                }
            }

            if (new List<string>(args).Contains("--rasterize-tmx")){

                if (Configuration.gbdkInstallationPath != null && Configuration.tiledInstallationPath != null){

                    // Make sure these two exist for these objects
                    if (!File.Exists(Configuration.gbdkInstallationPath + "/bin/png2asset.exe")) throw new Exception("PNG2asset.exe could not be found at "+ Configuration.gbdkInstallationPath+"/bin.");
                    if (!File.Exists(Configuration.tiledInstallationPath + "/tmxrasterizer.exe")) throw new Exception("tmxrasterizer.exe could not be found at " + Configuration.tiledInstallationPath+". Please make sure you have the latest tiled version.");

                    rasterizeTMXFiles = true;

                }
            }
        }

        /// <summary>
        /// Gets the absolute path to where .c source code files should go
        /// </summary>
        public static String FullSourceOutPath
        {
            get
            {
                // Did they provide a rooted path? 
                bool rooted = Path.IsPathRooted(sourcePath) && !Path.GetPathRoot(sourcePath).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);

                // Use the current directory if we weren't given a full path
                return rooted ? sourcePath : Environment.CurrentDirectory + "/" + sourcePath;
            }
        }

        /// <summary>
        /// Gets the absolute path to where .h header code files should go
        /// </summary>
        public static String FullHeaderOutPath
        {
            get
            {
                // Did they provide a rooted path?
                bool rooted = Path.IsPathRooted(headersPath) && !Path.GetPathRoot(headersPath).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);

                // Use the current directory if we weren't given a full path
                return rooted ? headersPath : Environment.CurrentDirectory+"/"+headersPath;
            }
        }


    }

}
