@echo off

:: Cleanup
rmdir /s /q bin
rmdir /s /q dist
rmdir /s /q src/gen
rmdir /s /q headers/gen

:: Cleanup
mkdir bin
mkdir dist


"../bin/debug/netcoreapp3.1/tiled2asset.exe" --headers-path "headers/gen" --source-path "src/gen" --tmx-dir "./tiled" --object-property uint8_t type 0 --object-property uint8_t direction J_DOWN --gbdk-installation-path "C:/gbdk" --tiled-installation-path "C:/progra~1/tiled" --rasterize-tmx --generate-map-struct --generate-object-struct --generate-string-lookup-function --export-strings

SET GBDK_HOME=C:/gbdk

SET LCC_COMPILE_BASE=%GBDK_HOME%\bin\lcc -debug -Iheaders/gen -Wa-l -Wl-m -Wl-j -DUSE_SFR_FOR_REG
SET LCC_COMPILE=%LCC_COMPILE_BASE% -c -o 

:: Required to concatenate the "COMPILE_OBJECT_FILES" via a for loop
SETLOCAL ENABLEDELAYEDEXPANSION

SET "COMPILE_OBJECT_FILES="
:: loop for all files in the default source folder
FOR /R "src/main" %%X IN (*.c) DO (
    SET "COMPILE_OBJECT_FILES=%%X !COMPILE_OBJECT_FILES!"

)

:: loop for all files in the default source folder
FOR /R "src/gen" %%X IN (*.c) DO (
    SET "COMPILE_OBJECT_FILES=%%X !COMPILE_OBJECT_FILES!"

)


:: Compile a .gb file from the compiled .o files
%LCC_COMPILE_BASE% -Wm-yc -mgbz80:gb -o dist/Tiled2AssetTest_Gameboy.gb !COMPILE_OBJECT_FILES!
%LCC_COMPILE_BASE% -Wm-yc -mgbz80:ap -o dist/Tiled2AssetTest_AnaloguePocket.pocket !COMPILE_OBJECT_FILES!
%LCC_COMPILE_BASE% -Wm-yc -mgbz80:duck -o dist/Tiled2AssetTest_MegaDuck.duck !COMPILE_OBJECT_FILES!
%LCC_COMPILE_BASE% -Wm-yc -mz80:sms -o dist/Tiled2AssetTest_SegaMasterSystem.sms !COMPILE_OBJECT_FILES!
%LCC_COMPILE_BASE% -Wm-yc -mz80:gg -o dist/Tiled2AssetTest_GameGear.gg !COMPILE_OBJECT_FILES!

endlocal
