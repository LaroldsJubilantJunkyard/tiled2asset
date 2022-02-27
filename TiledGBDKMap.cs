﻿using System;
using TiledCS;
using System.Collections.Generic;
using System.IO;

namespace tiled2Asset
{
    public class TiledGBDKMap : IComparable<TiledGBDKMap>
    {
        private const string TILED_GBDK_PROPERTY_PREFIX = "gbdk.";

        private string tmxFile;
        private TiledMap map;
        private int order=255;

        public TiledGBDKMap(string filename,int order)
        {
            this.tmxFile = filename;
            this.map = new TiledMap(filename);
            this.order = order;


            WriteTMXFile();
        }
        public TiledGBDKMap(string filename)
        {
            this.tmxFile = filename;
            this.map = new TiledMap(filename);
            this.order = 255;


            WriteTMXFile();
        }
        public  void WriteTMXFile()
        {

            List<string> mapObjectStructDefinitions = new List<string>();
            List<string> structStringParams = new List<string>();
            List<string> messages = new List<string>();
            List<TiledObject> gbdkExportEnabledTiledObjects = new List<TiledObject>();
            Dictionary<int, Dictionary<string, Object>> tiledExportObjectData = new Dictionary<int, Dictionary<string, Object>>();



            // Get all layers with objects in them
            List<TiledLayer> objectsLayers = new List<TiledLayer>(map.Layers).FindAll(x => x.objects != null && x.objects.Length > 0);

            // Get all objects that are gbdkExport enabled
            foreach (TiledLayer objectLayer in objectsLayers){
                gbdkExportEnabledTiledObjects.AddRange(new List<TiledObject>(objectLayer.objects).FindAll(x => HasGBDKEnabledProperty(x)));
            }


            foreach (TiledObject tiledObject in gbdkExportEnabledTiledObjects)
            {

                if (!tiledExportObjectData.ContainsKey(tiledObject.id))
                {
                    tiledExportObjectData[tiledObject.id] = new Dictionary<string, Object>();

                    // Always add x and y coordinates
                    tiledExportObjectData[tiledObject.id]["x"] = Math.Floor(tiledObject.x);
                    tiledExportObjectData[tiledObject.id]["y"] = Math.Floor(tiledObject.y);

                    // Add all of our default properties
                    foreach (KeyValuePair<string, string> objectProperty in Configuration.gbdkObjectDefaultProperties)
                    {
                        tiledExportObjectData[tiledObject.id][objectProperty.Key] = objectProperty.Value;
                    }
                }

                // For each Property in this object
                foreach (TiledProperty prop in tiledObject.properties)
                {
                    // If this is a message
                    // TODO
                    if (prop.name == "message")
                    {
                        tiledExportObjectData[tiledObject.id].Add("message", messages.Count);
                        messages.Add(prop.value);
                    }
                    else if (prop.name.StartsWith(TILED_GBDK_PROPERTY_PREFIX))
                    {
                        string gbdkProp = prop.name.Substring(5);

                        if (!Configuration.gbdkObjectDefaultProperties.ContainsKey(gbdkProp))
                        {
                            Console.WriteLine("Extra property specified on " + tiledObject.id + "(" + tiledObject.name + "): " + gbdkProp + ". All properties must be passed in via --object-props parameters, along with a default name and default value.");
                        }
                        else
                        {
                            tiledExportObjectData[tiledObject.id][gbdkProp] = prop.value;

                        }
                    }
                }
            }

            // Get the struct definition string for each export object
            foreach (KeyValuePair<int, Dictionary<string, Object>> pair in tiledExportObjectData){
                mapObjectStructDefinitions.Add(Utils.GetStructDefinition(pair.Value));
            }

            #region Write .h file

            List<string> hFileLines = new List<string>();
            hFileLines.Add("#include <gbdk/platform.h>");
            hFileLines.Add("#include <gbdk/metasprites.h>");
            hFileLines.AddRange(Configuration.headers);
            hFileLines.Add("#define " + mapIdentifier + "_OBJECT_COUNT " + tiledExportObjectData.Count);
            hFileLines.Add("extern const " + Configuration.gbdkMapStructName + " " + mapIdentifier + ";");

            if (Configuration.generateObjectStruct)
            {
                List<string> structDefinitions = new List<string>();


                foreach (string key in Configuration.gbdkObjectDefaultProperties.Keys)
                {
                    string type = Configuration.gbdkObjectDefaultPropertyTypes[key];
                    structDefinitions.Add(type + " " + key);
                }

                structDefinitions.Add("uint16_t x");
                structDefinitions.Add("uint16_t y");

                hFileLines.Add("\n#ifndef TILED_GBDK_OBJECT_STRUCT");
                hFileLines.Add("#define TILED_GBDK_OBJECT_STRUCT set");
                hFileLines.Add("typdef struct TiledGBDKObject {\n\t" + string.Join(";\n\t", structDefinitions) + "\n} TiledGBDKMap;"); ;
                hFileLines.Add("#endif");
            }

            if (Configuration.generateMapStruct)
            {
                List<string> structDefinitions = new List<string>(); 


                foreach(string key in Configuration.gbdkMapDefaultProperties.Keys)
                {
                    string type = Configuration.gbdkMapDefaultPropertyTypes[key];
                    structDefinitions.Add(type + " " + key);
                }

                if (Configuration.rasterizeTMXFiles)
                {
                    structDefinitions.Add("uint8_t *tileData");
                    structDefinitions.Add("uint8_t tileCount");
                    structDefinitions.Add("palette_color_t palettes");
                    structDefinitions.Add("uint8_t paletteCount");
                    structDefinitions.Add("uint8_t *map");
                    structDefinitions.Add("uint8_t *mapAttributes");
                }

                structDefinitions.Add("uint8_t objectCount");
                structDefinitions.Add("uint16_t widthInTiles");
                structDefinitions.Add("uint16_t heightInTiles");
                structDefinitions.Add(Configuration.gbdkObjectStructName + "[] objects");

                hFileLines.Add("\n#ifndef TILED_GBDK_MAP_STRUCT");
                hFileLines.Add("#define TILED_GBDK_MAP_STRUCT set");
                hFileLines.Add("typdef struct TiledGBDKMap {\n\t"+string.Join(";\n\t",structDefinitions)+"\n} TiledGBDKMap;"); ;
                hFileLines.Add("#endif");
            }

            File.WriteAllLines(mapIdentifier + ".h", hFileLines);

            #endregion

            #region Write .c file


            List<string> cFileLines = new List<string>();
            cFileLines.Add("#include <gbdk/platform.h>");
            cFileLines.Add("#include <gbdk/metasprites.h>");
            cFileLines.Add("#include \"" + mapIdentifier + ".h\"");
            cFileLines.AddRange(Configuration.headers);
            if (Configuration.rasterizeTMXFiles){
                cFileLines.Add("#include \""+mapIdentifier+"_tilemap.h\"");
            }

            foreach (TiledProperty prop in map.Properties)
            {
                if (prop.name.StartsWith("gbdkHeader"))
                {
                    cFileLines.Add(prop.value);
                }
            }



            Dictionary<string, Object> mapProperties = new Dictionary<string, object>();
            mapProperties.Add("objects", "{" + String.Join(",\n\t", mapObjectStructDefinitions) + "}");
            mapProperties.Add("objectCount", mapObjectStructDefinitions.Count);
            mapProperties.Add("widthInTiles", map.Width);
            mapProperties.Add("heightInTiles", map.Height);

            if (Configuration.rasterizeTMXFiles){
                mapProperties.Add("tileData", mapIdentifier + "_tilemap_tiles");
                mapProperties.Add("tileCount", mapIdentifier + "_tilemap_TILE_COUNT");
                mapProperties.Add("palettes", mapIdentifier + "_tilemap_palettes");
                mapProperties.Add("paletteCount", mapIdentifier + "_tilemap_PALETTE_COUNT");
                mapProperties.Add("map", mapIdentifier + "_tilemap_map");
                mapProperties.Add("mapAttributes", mapIdentifier + "_tilemap_map_attribues");
            }

            foreach (TiledProperty prop in map.Properties)
            {
                if (prop.name.StartsWith(TILED_GBDK_PROPERTY_PREFIX))
                {
                    string gbdkProp = prop.name.Substring(5);

                    if (!Configuration.gbdkMapDefaultProperties.ContainsKey(gbdkProp))
                    {
                        Console.WriteLine("Extra property specified on " + mapIdentifier + ": " + gbdkProp + ". All properties must be passed in via --map-property parameters, along with a default name and default value.");
                    }
                    else
                    {
                        mapProperties.Add(gbdkProp,prop.value);

                    }
                }else if (prop.name == "gbdkOrder")
                {
                    this.order = int.Parse(prop.value);
                }
            }
;
            cFileLines.Add("const " + Configuration.gbdkMapStructName + " " + mapIdentifier + "="+Utils.GetStructDefinition(mapProperties)+";");


            File.WriteAllLines(mapIdentifier + ".c", cFileLines);

            #endregion

        }

        /// <summary>
        /// Rasterizes the tmx file, with the objects hidden
        /// After rasterization, it converts that PNG to a GBDK asset
        /// </summary>
        public void RasterizeTMXFile()
        {
            List<string> hiddenLayers = new List<string>();


            // Get all layers with objects in them
            // Add them to a list with the --hide-layer parameter prepended
            List<TiledLayer> objectsLayers = new List<TiledLayer>(map.Layers).FindAll(x => x.objects != null && x.objects.Length > 0);
            foreach (TiledLayer layer in objectsLayers) hiddenLayers.Add("--hide-layer \""+layer.name+"\"");

            Utils.RunProcess(Configuration.tiledInstallationPath + "/tmxrasterizer.exe", String.Join(" ",hiddenLayers)+" \""+tmxFile+"\" " + mapIdentifier + "_tilemap.png");
            Utils.RunProcess(Configuration.gbdkInstallationPath + "/bin/png2asset.exe", mapIdentifier + "_tilemap.png -c "+ mapIdentifier + "_tilemap.c -map -use_map_attributes ");


        }


        /// <summary>
        /// Gets if a given tiled object has a property called "gbdkExport".
        /// The value of the property is not checked.
        /// </summary>
        /// <param name="tiledObject">The tiled object to check.</param>
        /// <returns>True, if the given tiledobject has the previously mentioned gbdkExport property</returns>
        public  bool HasGBDKEnabledProperty(TiledObject tiledObject)
        {
            foreach (TiledProperty prop in tiledObject.properties){
                if (prop.name == "gbdkExport") return true;
            }


            return false;
        }

        /// <summary>
        /// Sets up how to compare to instances ofthis class. Compare by order, then by map identifier
        /// </summary>
        /// <param name="other">The other TiledGBDKMap to compare with</param>
        /// <returns></returns>
        public int CompareTo( TiledGBDKMap other)
        {
            int val= order.CompareTo(other.order);

            // If we have the same order value
            // Compare using our map identifiers
            if (val == 0) return mapIdentifier.CompareTo(other.mapIdentifier);

            // Use our order value
            return val;
        }

        public string mapIdentifier{
            get{

                string id = tmxFile.Substring(0, tmxFile.LastIndexOf(".tmx"));
                id = id.Replace("\\", "/");
                id = id.Split("/")[id.Split("/").Length - 1];
                return id;
            }
        }
    }


}