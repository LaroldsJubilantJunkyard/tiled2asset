using System;
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
        public int order=255;


        List<string> mapObjectStructDefinitions;
        List<string> structStringParams;
        List<string[]> messages;
        List<string[]> mapMessages;
        List<TiledObject> gbdkExportEnabledTiledObjects;
        Dictionary<int, Dictionary<string, Object>> tiledExportObjectData ;
        List<TiledLayer> objectsLayers;

        public TiledGBDKMap(string filename,int order)
        {
            this.tmxFile = filename;
            this.map = new TiledMap(filename);
            this.order = order;


        }
        public TiledGBDKMap(string filename)
        {
            this.tmxFile = filename;
            this.map = new TiledMap(filename);
            this.order = 255;


        }
        public  void WriteTMXFile()
        {

            mapObjectStructDefinitions = new List<string>();
            structStringParams = new List<string>();
            messages = new List<string[]>();
            mapMessages = new List<string[]>();
            gbdkExportEnabledTiledObjects = new List<TiledObject>();
            tiledExportObjectData = new Dictionary<int, Dictionary<string, Object>>();

            // Get all layers with objects in them
            objectsLayers = new List<TiledLayer>(map.Layers).FindAll(x => x.objects != null && x.objects.Length > 0);

            // Get all objects that are gbdkExport enabled
            foreach (TiledLayer objectLayer in objectsLayers){
                gbdkExportEnabledTiledObjects.AddRange(new List<TiledObject>(objectLayer.objects).FindAll(x => HasGBDKEnabledProperty(x)));
            }


            foreach (TiledObject tiledObject in gbdkExportEnabledTiledObjects)
            {

                if (!tiledExportObjectData.ContainsKey(tiledObject.id))
                {
                    string name = (tiledObject.name == null || tiledObject.name.Trim().Length == 0) ? "Object" + tiledObject.id : tiledObject.name;

                    tiledExportObjectData[tiledObject.id] = new Dictionary<string, Object>();

                    // Always add x and y coordinates
                    tiledExportObjectData[tiledObject.id]["x"] = Math.Floor(tiledObject.x);
                    tiledExportObjectData[tiledObject.id]["y"] = Math.Floor(tiledObject.y);
                    tiledExportObjectData[tiledObject.id]["id"] = mapIdentifier+"_"+name;

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
                    if (prop.name.StartsWith("gbdkString."))
                    {
                        string gbdkProp = prop.name.Substring(11);
                        string name = (tiledObject.name == null || tiledObject.name.Trim().Length == 0) ? "Object" + tiledObject.id : tiledObject.name;
                        messages.Add(new string[] {name, gbdkProp, prop.value });
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


            // For each Property in this map
            foreach (TiledProperty prop in map.Properties)
            {
                // If this is a message
                if (prop.name.StartsWith("gbdkString."))
                {
                    string gbdkProp = prop.name.Substring(11);
                    mapMessages.Add(new string[] {gbdkProp, prop.value });
                }
            }

            // Get the struct definition string for each export object
            foreach (KeyValuePair<int, Dictionary<string, Object>> pair in tiledExportObjectData){
                mapObjectStructDefinitions.Add(Utils.GetStructDefinition(pair.Value));
            }

            WriteHFile();
            WriteCFile();

        }

        private void WriteStructHFile()
        {
            List<string> hFileLines = new List<string>();
            hFileLines.Add("// This file was generated by tiled2asset");
            hFileLines.Add("#include <gbdk/platform.h>");
            hFileLines.Add("#include <gbdk/metasprites.h>");
            hFileLines.AddRange(Configuration.headers);

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
                structDefinitions.Add("uint16_t id");

                hFileLines.Add("\n#ifndef TILED_GBDK_OBJECT_STRUCT");
                hFileLines.Add("#define TILED_GBDK_OBJECT_STRUCT set");
                hFileLines.Add("typedef struct " + Configuration.gbdkObjectStructName + " {\n\t" + string.Join(";\n\t", structDefinitions) + ";\n} " + Configuration.gbdkObjectStructName + ";"); ;
                hFileLines.Add("#endif");


                File.WriteAllLines(Configuration.gbdkObjectStructName + ".h", hFileLines);
            }

            hFileLines.Clear();
            hFileLines.Add("// This file was generated by tiled2asset");
            hFileLines.Add("#include <gbdk/platform.h>");
            hFileLines.Add("#include <gbdk/metasprites.h>"); 
            hFileLines.Add("#include \"" + Configuration.gbdkObjectStructName + ".h\"");

            hFileLines.AddRange(Configuration.headers);

            if (Configuration.generateMapStruct)
            {
                List<string> structDefinitions = new List<string>();


                if (Configuration.generateStringLookupFunction)
                {
                    structDefinitions.Add("unsigned char* (*getGBDKString)(uint16_t objectId, unsigned char* string)");
                }


                foreach (string key in Configuration.gbdkMapDefaultProperties.Keys)
                {
                    string type = Configuration.gbdkMapDefaultPropertyTypes[key];
                    structDefinitions.Add(type + " " + key);
                }

                if (Configuration.rasterizeTMXFiles)
                {
                    structDefinitions.Add("uint8_t *tileData");
                    structDefinitions.Add("uint8_t tileCount");
                    structDefinitions.Add("palette_color_t *palettes");
                    structDefinitions.Add("uint8_t paletteCount");
                    structDefinitions.Add("uint8_t *map");
                    structDefinitions.Add("uint8_t *mapAttributes");
                }

                structDefinitions.Add("uint8_t objectCount");
                structDefinitions.Add("uint16_t widthInTiles");
                structDefinitions.Add("uint16_t heightInTiles");
                structDefinitions.Add("uint8_t id");
                structDefinitions.Add(Configuration.gbdkObjectStructName + " objects[]");

                hFileLines.Add("\n#ifndef TILED_GBDK_MAP_STRUCT");
                hFileLines.Add("#define TILED_GBDK_MAP_STRUCT set");
                hFileLines.Add("typedef struct " + Configuration.gbdkMapStructName + " {\n\t" + string.Join(";\n\t", structDefinitions) + ";\n} TiledGBDKMap;"); ;
                hFileLines.Add("#endif");
                File.WriteAllLines(Configuration.gbdkMapStructName + ".h", hFileLines);
            }

        }

        private void WriteHFile()
        {
            List<string> hFileLines = new List<string>();
            hFileLines.Add("// This file was generated by tiled2asset");
            hFileLines.Add("#include <gbdk/platform.h>");
            hFileLines.Add("#include <gbdk/metasprites.h>");
            if (Configuration.generateObjectStruct){
                hFileLines.Add("#include \"" + Configuration.gbdkObjectStructName + ".h\"");
            }
            if (Configuration.generateMapStruct) { 
                hFileLines.Add("#include \"" + Configuration.gbdkMapStructName + ".h\"");
            }
            hFileLines.AddRange(Configuration.headers);
            hFileLines.Add("#define " + mapIdentifier + "_WIDTH " + map.Width);
            hFileLines.Add("#define " + mapIdentifier + "_HEIGHT " + map.Height);

            // Get the struct definition string for each export object
            foreach (KeyValuePair<int, Dictionary<string, Object>> pair in tiledExportObjectData)
            {
                TiledObject obj = GetTiledObjectById(pair.Key);


                string name = (obj.name == null || obj.name.Trim().Length == 0) ? "Object" + obj.id : obj.name;

                hFileLines.Add("#define " + mapIdentifier + "_" + name + " " + obj.id);
            }
            hFileLines.Add("#define TILEDMAP_" + mapIdentifier + " " + order);

            if (Configuration.generateObjectStruct||Configuration.generateMapStruct)
            {
                WriteStructHFile();
            }

            hFileLines.Add("extern const " + Configuration.gbdkMapStructName + " " + mapIdentifier + ";");

            if (Configuration.exportObjectStrings)
            {
                foreach (string[] pair in mapMessages)
                {
                    hFileLines.Add("extern const unsigned char " + mapIdentifier + "_" + pair[0] + "[" + pair[1].Length + "];");
                }
                foreach (string[] pair in messages)
                {
                    hFileLines.Add("extern const unsigned char " + mapIdentifier + "_" + pair[0] + "_" + pair[1] + "[" + pair[2].Length + "];");
                }
            }


            if (Configuration.generateStringLookupFunction)
            {
                foreach (string[] pair in messages)
                {
                    hFileLines.Add("extern const unsigned char " + mapIdentifier + "_" + pair[0] + "_" + pair[1] + "[" + pair[2].Length + "];");
                }
            }
            if (Configuration.generateStringLookupFunction)
            {

                hFileLines.Add("extern unsigned char* " + mapIdentifier + "_GetGBDKString(uint16_t objectId, unsigned char* string);");
            }

            File.WriteAllLines(mapIdentifier + ".h", hFileLines);
        }

        void WriteCFile()
        {

            List<string> cFileLines = new List<string>();
            cFileLines.Add("// This file was generated by tiled2asset");
            cFileLines.Add("#include <gbdk/platform.h>");
            cFileLines.Add("#include <gbdk/metasprites.h>");
            if (Configuration.generateObjectStruct){
                cFileLines.Add("#include \"" + Configuration.gbdkObjectStructName + ".h\"");
            }
            if (Configuration.generateMapStruct) {
                cFileLines.Add("#include \"" + Configuration.gbdkMapStructName + ".h\"");
            }

            if (Configuration.generateStringLookupFunction)
            {

                cFileLines.Add("#include <string.h>");
            }
            cFileLines.Add("#include \"" + mapIdentifier + ".h\"");
            cFileLines.AddRange(Configuration.headers);
            if (Configuration.rasterizeTMXFiles)
            {
                cFileLines.Add("#include \"" + mapIdentifier + "_tilemap.h\"");
            }

            foreach (TiledProperty prop in map.Properties)
            {
                if (prop.name.StartsWith("gbdkHeader"))
                {
                    cFileLines.Add(prop.value);
                }
            }


            int count = 0;


            if (Configuration.exportObjectStrings)
            {
                foreach (string[] pair in mapMessages)
                {
                    cFileLines.Add("const unsigned char " + mapIdentifier + "_" + pair[0] + "[" + pair[1].Length + "]=\"" + pair[1] + "\";");
                }
                foreach (string[] pair in messages)
                {
                    cFileLines.Add("const unsigned char " + mapIdentifier + "_" + pair[0] + "_" + pair[1] + "[" + pair[2].Length + "]=\"" + pair[2] + "\";");
                }
            }

            if (Configuration.generateStringLookupFunction)
            {

                int statements = 0;

                cFileLines.Add("unsigned char * " + mapIdentifier + "_GetGBDKString(uint16_t objectId, unsigned char * string) {");
                // Get the struct definition string for each export object
                foreach (KeyValuePair<int, Dictionary<string, Object>> pair in tiledExportObjectData)
                {
                    TiledObject obj = GetTiledObjectById(pair.Key);
                    string name = (obj.name == null || obj.name.Trim().Length == 0) ? "Object" + obj.id : obj.name;

                    List<string[]> objMessages = new List<string[]>();

                    foreach (TiledProperty prop in obj.properties)
                    {
                        if (prop.name.StartsWith("gbdkString."))
                        {

                            string gbdkProp = prop.name.Substring(11);
                            objMessages.Add(new string[] { gbdkProp, prop.value });
                        }
                    }

                    if (objMessages.Count > 0)
                    {
                        cFileLines.Add("\t" + (statements > 0 ? "else " : "") + " if(objectId == " + mapIdentifier + "_" + name + "){");
                        foreach (string[] objMsg in objMessages)
                        {
                            cFileLines.Add("\t\tif(strcmp(string,\"" + objMsg[0] + "\")==0)return " + mapIdentifier + "_" + name + "_" + objMsg[0] + ";");
                        }
                        cFileLines.Add("\t}");

                        statements++;
                    }


                }

                if (mapMessages.Count > 0)
                {
                    cFileLines.Add("\tif(objectId==0xFFFF){");
                    foreach (string[] mapMsg in mapMessages)
                    {
                        cFileLines.Add("\t\tif(strcmp(string,\"" + mapMsg[0] + "\")==0)return " + mapIdentifier + "_" + mapMsg[0] + ";");
                    }
                    cFileLines.Add("\t};");
                }


                cFileLines.Add("\treturn 0;");
                cFileLines.Add("}");

            }


            Dictionary<string, Object> mapProperties = new Dictionary<string, object>();
            mapProperties.Add("objects", "{" + String.Join(",\n\t", mapObjectStructDefinitions) + "}");
            mapProperties.Add("objectCount", mapObjectStructDefinitions.Count);
            mapProperties.Add("widthInTiles", mapIdentifier+"_WIDTH");
            mapProperties.Add("heightInTiles", mapIdentifier + "_HEIGHT");
            mapProperties.Add("id", "TILEDMAP_"+mapIdentifier);
            if (Configuration.generateStringLookupFunction){

                mapProperties.Add("getGBDKString", "&"+mapIdentifier+"_GetGBDKString");
            }

            if (Configuration.rasterizeTMXFiles)
            {
                mapProperties.Add("tileData", mapIdentifier + "_tilemap_tiles");
                mapProperties.Add("tileCount", mapIdentifier + "_tilemap_TILE_COUNT");
                mapProperties.Add("palettes", mapIdentifier + "_tilemap_palettes");
                mapProperties.Add("paletteCount", mapIdentifier + "_tilemap_PALETTE_COUNT");
                mapProperties.Add("map", mapIdentifier + "_tilemap_map");
                mapProperties.Add("mapAttributes", mapIdentifier + "_tilemap_map_attributes");
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
                        mapProperties.Add(gbdkProp, prop.value);

                    }
                }
                else if (prop.name == "gbdkOrder")
                {
                    this.order = int.Parse(prop.value);
                }
            }
;
            cFileLines.Add("const " + Configuration.gbdkMapStructName + " " + mapIdentifier + "=" + Utils.GetStructDefinition(mapProperties) + ";");


            File.WriteAllLines(mapIdentifier + ".c", cFileLines);
        }

        public TiledObject GetTiledObjectById(int id)
        {

            // Get all layers with objects in them
            List<TiledLayer> objectsLayers = new List<TiledLayer>(map.Layers).FindAll(x => x.objects != null && x.objects.Length > 0);

            // Get all objects that are gbdkExport enabled
            foreach (TiledLayer objectLayer in objectsLayers)
            {
                TiledObject obj = new List<TiledObject>(objectLayer.objects).Find(x => x.id==id);

                if (obj != null) return obj;
            }

            return null;
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
