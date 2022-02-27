﻿# Tiled2Asset
This tool was written using C# for the [GBDK-2020](https://github.com/gbdk-2020) Gameboy Game Development Kit. It's purpose is to enable usage of the Map Editor [Tiled](https://www.mapeditor.org/) when making gameboy games. For more information on writing gameboy games with GBDK-2020, see the (GBDK-2020 Documentation)[https://gbdk-2020.github.io/gbdk-2020/docs/api/index.html].

Tiled2asset will export all objects that have a custom property named '**gbdkExport**' . It's value doesn't matter for now.

## Adding Tiled .tmx files
The '**--tmx-file**' and '**--tmx-dir**' parameters are used to add tiled files for tiled2asset to process. If '**--tmx-dir**' is used, the provided directory will be recursively searched for .tmx files. Order is not a given with this method. Maps can set their order using a 'gbdkOrder' custom property in tiled. Alternatively, maps can be individually passed in using the '**--tmx-file**' parameter. This parameter only takes in one specific tile .tmx file, but can also be passed in multiple times.

**The objects generated will match the tiled tmx filename.**

## Object & Map Structs
When it comes to maps and objects. Tiled2asset gives developers the option of using their own structs, or automatically generating structs based of the parameters given. If a developer want's to use his own structs, the proper #includes should be passed via the '**--gbdk-header**' parameter. 

### Generated Structs
Structs for the objecs and maps can be generated using the '**--generate-object-struct**' and '**--generate-map-struct**' parameter. For struct generation the map & object structs have a small amount of required properties.

**Maps**
 - objects
 - objectsCount
 - widthInTiles
 - heightInTiles

**Objects**
 - x
 - y

 By default, these will appear on all structs generated by tiled2asset. To add more properties, the '**--map-property**' and '**--object-property**' parameters must be used.
 
 ### Adding Properties
 The '**--map-property**' and '**--object-property**' act in a very similar manner. However the '**--map-property**' acts on maps, and the '**--object-property**' acts on objects found in those maps. Both require 3 following parameters: The GBDK/C type, the identifier, and a default value. When tiled2asset is scanning objects in tiled maps, it will look for custom properties prefixed with 'gbdk.' (example 'gbdk.type'). For each gbdk. prefixed custom property, if that property was passed into tiled2asset (via the '**--object-property**' or '**--map-property**' parameters) for the map or objects, it will be included when exporting objects. If the property is passed into tile2asset, but it is not found on a object

 ## Rasterizing The Levels

 You can optionally pass the '**--rasterize-tmx**' parameter. This parameter requires the '**--gbdk-installation-path**' and '**--tiled-installation-path**' parameters also be set. It uses the tmxrasterizer.exe (found in the latest releases) and png2asset (part of the GBDK-2020) to draw out each level and include it in the generated structs for each level. The following extra parameters will be added to each map object:

 - **tileCount** - How many unique tiles are in the map
 - **tileData** - A pointer to the start of the tile data
 - **paletteCount** - How many palettes are in the map.
 - **palettes** - A pointer to the array of palette's
 - **map** - The plane 0 tiles to use
 - **mapAttributes** - The plane 1 attributes for colors, priority, and tile flipping

## Options

**All Options**

 - **--help** Shows this help dialogue
 - **--tmx-dir <directory>** Where to recursively search for .tmx files. Can be done multiple times.
 - **--tmx-file <tmx-file>** A tiled .tmx level file to process. Can be done multiple times.
 - **--object-property <type> <name> <default>** Defines a property that all objects should have. Can be done multiple times. Tiled2asset will look for a custom property that is prefixed with '.gbdk'. When exporting, if not found on a object, it will use the default value.
 - **--map-property <type> <name> <default>** Defines a property that all maps should have. Can be done multiple times. Tiled2asset will look for a custom property that is prefixed with '.gbdk'. When exporting, if not found on a map, it will use the default value.
 - **--gbdk-header <header>** C include's to be added at the top of the .c file. Can be done multiple times
 - **--map-struct-name <name>** What already created struct to use for maps. Defaults to TiledGBDKMap.
 - **--object-struct-name <name>** What already created struct to use for objects.Defaults to TiledGBDKObject. 
 - **--gbdk-installation-path <name>** Where GBDK is installed
 - **--tiled-installation-path <name>** Where tiled is installed.
 - **--rasterize-tmx** When provided a GBDK & Tiled installation path, this renders the TMX tile layers to a PNG, and uses png2asset to render that to a GBDK map. That map will also be included in the structs generated for each map.
 - **--generate-object-struct** Generates a struct named 'TiledGBDKMap for maps.
 - **--generate-map-struct** Generates a struct named 'TiledGBDKObject' for objects.