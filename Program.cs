using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tiled2Asset
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            // If the user wanted to see the help
            if(new List<string>(args).Contains("--help")|| new List<string>(args).Contains("-h"))
            {
                Console.WriteLine("Tiled2Asset Help:");
                Console.WriteLine("The goal behind Tiled2Asset is simplifying the level design process for GBDK-2020 game development. ");
                Console.WriteLine("\t--help                                             Shows this help dialogue");
                Console.WriteLine("\t--tmx-dir <directory>                              A directory to recursively search for tile .tmx level files. Canbe done multiple times.");
                Console.WriteLine("\t--tmx-file <tmx-file>                              A tiled .tmx level file to process. Can be done multiple times.");
                Console.WriteLine("\t--object-property <type> <name> <default-value>    Defines a property that all objects should have, as well as it's default value and type. Can be done multiple times.");
                Console.WriteLine("\t--map_property <type> <name> <default-value>       Defines a property that all maps should have, as well as it's default value and type. Can be done multiple times.");
                Console.WriteLine("\t--gbdk-header <header>                             C #include's to be added at the top of the .c file. Can be done multiple times.");
                Console.WriteLine("\t--map-struct-name <name>                           What already created struct to use for maps. alternatively, --generate-map-struct can be used to automate this.");
                Console.WriteLine("\t--object-struct-name <name>                        What already created struct to use for objects. alternatively, --generate-object-struct can be used to automate this.");
                Console.WriteLine("\t--gbdk-installation-path <name>                    Where GBDK is installed");
                Console.WriteLine("\t--tiled-installation-path <name>                   Where tiled is installed.");
                Console.WriteLine("\t--rasterize-tmx                                    When provided a GBDK & Tiled installation path, this renders the TMX tile layers to a PNG, and uses png2asset to render that to a GBDK map");
                Console.WriteLine("\t--generate-object-struct                           Generates a struct named 'TiledGBDKMap for maps.");
                Console.WriteLine("\t--generate-map-struct                              Generates a struct named 'TiledGBDKObject' for objects.");
                Console.WriteLine("\nHelpful Resources:");
                Console.WriteLine("\tThe GBDK-2020 Library: https://github.com/gbdk-2020/gbdk-2020/");
                Console.WriteLine("\tDocumentation for GBDK-2020: https://gbdk-2020.github.io/gbdk-2020/docs/api/index.html");
                Console.WriteLine("\tGBDK/ZGB Discord Server: https://discord.gg/XCbjCvqnUY");
            }
            else
            {
                // Read our configuration from our arguments
                Configuration.ReadFromArgs(args);

                // 
                Tiled2AssetInstance tiled2AssetInstance = new Tiled2AssetInstance();
                tiled2AssetInstance.ExportTiledTMXFiles(args);
                tiled2AssetInstance.ExportAllLevelsFiles(args);

                // If we have the tiled and gbdk paths and the --rasterize-tmx parameter set
                // Rasterize our tmx files into images
                if (Configuration.rasterizeTMXFiles){
                    tiled2AssetInstance.RasterizeTMXFiles();
                }

            }
            

        }

       }


}
