#include <gbdk/platform.h>
#include "AllLevels.h"

uint8_t currentLevel=0;
uint16_t mapX=0,mapY=0;
uint8_t leftColumn,topRow;


uint8_t UpdateWorldCamera() {

    move_bkg(mapX>>4,mapY>>4);

    uint16_t newColumn=(mapX>>4)/8;
    uint16_t newRow=(mapY>>4)/8;


    const uint8_t *plane0=AllLevels[currentLevel]->map;
    const uint8_t *plane1 = AllLevels[currentLevel]->mapAttributes;
    const uint8_t mapWidth = AllLevels[currentLevel]->widthInTiles;
    uint8_t changed=FALSE;

    if(newColumn!=leftColumn){

        if(newColumn>leftColumn){
            VBK_REG=1; set_bkg_submap(newColumn+DEVICE_SCREEN_WIDTH,topRow,1,DEVICE_SCREEN_HEIGHT,plane1,mapWidth);
            VBK_REG=0; set_bkg_submap(newColumn+DEVICE_SCREEN_WIDTH,topRow,1,DEVICE_SCREEN_HEIGHT,plane0,mapWidth);
        }else{

            VBK_REG=1; set_bkg_submap(newColumn,topRow,1,DEVICE_SCREEN_HEIGHT,plane1,mapWidth);
            VBK_REG=0; set_bkg_submap(newColumn,topRow,1,DEVICE_SCREEN_HEIGHT,plane0,mapWidth);
        }
        leftColumn=newColumn;
        changed=TRUE;
    }

    if(newRow!=topRow){

        if(newRow>topRow){
            VBK_REG=1; set_bkg_submap(leftColumn,topRow+DEVICE_SCREEN_HEIGHT,DEVICE_SCREEN_WIDTH,1,plane1,mapWidth);
            VBK_REG=0; set_bkg_submap(leftColumn,topRow+DEVICE_SCREEN_HEIGHT,DEVICE_SCREEN_WIDTH,1,plane0,mapWidth);
        }else{

            VBK_REG=1; set_bkg_submap(leftColumn,topRow,DEVICE_SCREEN_WIDTH,1,plane1,mapWidth);
            VBK_REG=0; set_bkg_submap(leftColumn,topRow,DEVICE_SCREEN_WIDTH,1,plane0,mapWidth);
        }
        topRow=newRow;
        changed=TRUE;

    }
    

    return changed;
}

void main(void){

    DISPLAY_ON;
    SHOW_BKG;

    set_bkg_data(0,AllLevels[0]->tileCount,AllLevels[0]->tileData);
    set_bkg_palette(0,AllLevels[0]->paletteCount,AllLevels[0]->palettes);
    set_bkg_submap(0,0,DEVICE_SCREEN_WIDTH,DEVICE_SCREEN_HEIGHT,AllLevels[0]->map,AllLevels[0]->widthInTiles);

    uint8_t joypadCurrent=0;
    uint8_t joypadPrevious=0;
    uint8_t speed =10;

    while(1){

        joypadPrevious = joypadCurrent;
        joypadCurrent = joypad();

        if(joypadCurrent & J_LEFT){
            if(mapX>=speed)mapX-=speed;
            else mapX=0;
        }else if(joypadCurrent & J_RIGHT){
            if(mapX+((8*DEVICE_SCREEN_WIDTH)<<4)<=((AllLevels[currentLevel]->widthInTiles*8)<<4)-speed)mapX+=speed;
            else mapX=((AllLevels[currentLevel]->widthInTiles*8)<<4)-((8*DEVICE_SCREEN_WIDTH)<<4);
        }
        if(joypadCurrent & J_UP){
            if(mapY>=speed)mapY-=speed;
            else mapY=0;
        }else if(joypadCurrent & J_DOWN){
            if(mapY+((8*DEVICE_SCREEN_HEIGHT)<<4)<=((AllLevels[currentLevel]->heightInTiles*8)<<4)-speed)mapY+=speed;
            else mapY=((AllLevels[currentLevel]->heightInTiles*8)<<4)-((8*DEVICE_SCREEN_HEIGHT)<<4);
        }

        if((joypadCurrent & J_A) && !(joypadPrevious & J_A)){
            currentLevel = (currentLevel+1)%LEVEL_COUNT;
            set_bkg_data(0,AllLevels[currentLevel]->tileCount,AllLevels[currentLevel]->tileData);
            set_bkg_palette(0,AllLevels[currentLevel]->paletteCount,AllLevels[currentLevel]->palettes);
            set_bkg_submap(0,0,DEVICE_SCREEN_WIDTH,DEVICE_SCREEN_HEIGHT,AllLevels[currentLevel]->map,AllLevels[currentLevel]->widthInTiles);
            mapX=0;
            mapY=0;
        }

        UpdateWorldCamera();
        wait_vbl_done();
    }
}