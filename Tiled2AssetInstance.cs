﻿using System.Collections.Generic;
using System.IO;

namespace tiled2Asset
{
    public class Tiled2AssetInstance
    {

        List<TiledGBDKMap> tmxFiles = new List<TiledGBDKMap>();

        /// <summary>
        /// Gets all TMX files directly specified (via --tmx-file), and/or that may exist in any --tmx-dir's specified. Those files
        /// are then sorted.
        /// </summary>
        /// <param name="args"></param>
        public void ExportTiledTMXFiles(string[] args)
        {
            // For each argument, in orer
            for (int i = 0; i < args.Length; i++)
            {
                // Can be done multiple times
                // A single tmx to convert for GBDK 2020
                if (args[i] == "--tmx-file"){

                    // Add to the list with it's order in the list as it's order
                    tmxFiles.Add(new TiledGBDKMap(args[i + 1], tmxFiles.Count));
                    i++;
                }

                // Can be done multiple times
                // A path to recursively search for tmx files
                else if (args[i] == "--tmx-dir")
                {

                    // recursivly get all .tmx files
                    string[] files = Directory.GetFiles(args[i + 1], "*.tmx", SearchOption.AllDirectories);

                    for (int j = 0; j < files.Length; j++){

                        // Do not explicitly set an order
                        tmxFiles.Add(new TiledGBDKMap(files[j]));

                    }

                }
            }

            // Sort based on order
            tmxFiles.Sort();
        }

        public void ExportAllLevelsFiles(string[] args)
        {


            #region Write AllLevels.h

            List<string> hFileLines = new List<string>();
            hFileLines.Add("#include <gbdk/platform.h>");
            hFileLines.Add("#include <gbdk/metasprites.h>");
            hFileLines.AddRange(Configuration.headers);
            hFileLines.Add("#define LEVEL_COUNT " + tmxFiles.Count);
            hFileLines.Add("extern const " + Configuration.gbdkMapStructName + " AllLevels[LEVEL_COUNT];");
            File.WriteAllLines("AllLevels.h", hFileLines);

            #endregion

            #region Write AllLevels.c

            List<string> cFileLines = new List<string>();
            cFileLines.Add("#include <gbdk/platform.h>");
            cFileLines.Add("#include <gbdk/metasprites.h>");

            // Include all levels
            foreach (TiledGBDKMap tmxFile in tmxFiles){
                cFileLines.Add("#include \"" + tmxFile.mapIdentifier + ".h\"");
            }

            // Write each level to the all levels array
            cFileLines.Add("const " + Configuration.gbdkMapStructName + " AllLevels[" + tmxFiles.Count + "] = {");
            foreach (TiledGBDKMap tmxFile in tmxFiles){
                cFileLines.Add("\t&" + tmxFile.mapIdentifier + ",");
            }
            cFileLines.Add("};");
            File.WriteAllLines("AllLevels.c", cFileLines);

            #endregion
        }


        public void RasterizeTMXFiles()
        {
            // Include all levels
            foreach (TiledGBDKMap tmxFile in tmxFiles)
            {
                tmxFile.RasterizeTMXFile();
            }
        }
    }


}
