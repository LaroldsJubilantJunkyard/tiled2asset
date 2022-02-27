using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tiled2Asset
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // If the user wanted to see the help
            if(new List<string>(args).Contains("--help")|| new List<string>(args).Contains("-h"))
            {
                Console.WriteLine("Tiled2Asset Options");

                Console.WriteLine("\t--help                                     Shows this help dialogue\n");
                Console.WriteLine("\t--tmx-dir <directory>                      Where to recursively search for .tmx files. Can be done multiple times.\n");
                Console.WriteLine("\t--tmx-file <tmx-file>                      A tiled .tmx level file to process. Can be done multiple times.\n");
                Console.WriteLine("\t--object-property <type> <name> <default>  Defines a property that all objects should have. Can be done multiple times.");
                Console.WriteLine("\t                                           Tiled2asset will look for a custom property that is prefixed with '.gbdk'.");
                Console.WriteLine("\t                                           When exporting, if not found on a object, it will use the default value.\n");
                Console.WriteLine("\t--map-property <type> <name> <default>     Defines a property that all maps should have. Can be done multiple times.");
                Console.WriteLine("\t                                           Tiled2asset will look for a custom property that is prefixed with '.gbdk'.");
                Console.WriteLine("\t                                           When exporting, if not found on a map, it will use the default value.\n");
                Console.WriteLine("\t--gbdk-header <header>                     C #include's to be added at the top of the .c file. Can be done multiple times.\n");
                Console.WriteLine("\t--map-struct-name <name>                   What already created struct to use for maps. Defaults to TiledGBDKMap. \n");
                Console.WriteLine("\t--object-struct-name <name>                What already created struct to use for objects.Defaults to TiledGBDKObject. \n");
                Console.WriteLine("\t--gbdk-installation-path <name>            Where GBDK is installed\n");
                Console.WriteLine("\t--tiled-installation-path <name>           Where tiled is installed.\n");
                Console.WriteLine("\t--rasterize-tmx                            When provided a GBDK & Tiled installation path, this renders the TMX tile layers");
                Console.WriteLine("\t                                           to a PNG, and uses png2asset to render that to a GBDK map. That may is\n");
                Console.WriteLine("\t--generate-object-struct                   Generates a struct named 'TiledGBDKMap for maps.\n");
                Console.WriteLine("\t--generate-map-struct                      Generates a struct named 'TiledGBDKObject' for objects.\n");
                Console.WriteLine("\t--export-strings                           Generates constants for each 'gbdkString.' prefixed custom.\n");
                Console.WriteLine("\t                                           property found on the map or an object.\n");
                Console.WriteLine("\t--generate-string-lookup-function          Generates a string lookup function for each map. Also adding a  pointer to.\n");
                Console.WriteLine("\t                                           this function in the map's struct and struct definition.\n");
                Console.WriteLine("\t--source-path <directory>                  Where to put .c files.\n");
                Console.WriteLine("\t--headers-path <directory>                 Where to put .h files.\n");
                Console.WriteLine("");
                Console.WriteLine("\nHelpful Resources:");
                Console.WriteLine("\tThe GBDK-2020 Library: https://github.com/gbdk-2020/gbdk-2020/");
                Console.WriteLine("\tDocumentation for GBDK-2020: https://gbdk-2020.github.io/gbdk-2020/docs/api/index.html");
                Console.WriteLine("\tGBDK/ZGB Discord Server: https://discord.gg/XCbjCvqnUY");
            }
            else
            {
                // Read our configuration from our arguments
                Configuration.ReadFromArgs(args);

                // Create a new instance
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
