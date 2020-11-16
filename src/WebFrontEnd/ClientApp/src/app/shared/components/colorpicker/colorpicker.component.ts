import { Component, OnInit, Input, Inject } from '@angular/core';
import {MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA} from '@angular/material/bottom-sheet';

const RGB_MIN = 0;
const RGB_MAX = 255;
const HEX_MIN = 0;
const HEX_MAX = 360;
const LUMINOSITY_MIN = 10;
const LUMINOSITY_MAX = 90;
const SATURATION = 100;
const numColorsPerRow = 11;
const numShadeOfColor = 9;

interface ColorPickerData {
  handleColorChange: (color: string) => void
}

@Component({
  selector: 'app-colorpicker',
  templateUrl: './colorpicker.component.html',
  styleUrls: ['./colorpicker.component.scss']
})
export class ColorPickerComponent implements OnInit {
  @Input() isColorsSpecified : boolean = false
  @Input() priorityColors : string[]
  grayScaleColors : string[]
  rainbowColors: string[][]

  constructor(
    private _colorPickerRef: MatBottomSheetRef<ColorPickerComponent>, 
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: ColorPickerData){
    this.grayScaleColors = this.generateGrayScale(numColorsPerRow);
    this.rainbowColors = this.generateRainbow(numColorsPerRow, numShadeOfColor);
  }

  ngOnInit() {}

  pickColor(color: string) : void {
      console.log(color);
      this.data.handleColorChange(color);
      this._colorPickerRef.dismiss();
      event.preventDefault();
  }

  generateGrayScale = (numSquares: number) : string[] => {
    let grayScale = []

    for (let value = RGB_MIN; value <= RGB_MAX - RGB_MAX/numSquares; value += RGB_MAX/numSquares) {
      grayScale.push(`rgb(${value}, ${value}, ${value})`)
    }

    return grayScale
  }

  generateRainbow = (numColors : number, numSquares: number) : string[][] => {

    let rainbowColors = []
    let baseColors = []

    for (let hex = HEX_MIN; hex <= HEX_MAX - HEX_MAX/numColors; hex += HEX_MAX/numColors) {
      baseColors.push(hex);
    }

    for (let color of baseColors) {
      let colorRow = []
      for (let luminosity = LUMINOSITY_MIN; luminosity <= LUMINOSITY_MAX; luminosity += LUMINOSITY_MAX/numSquares) {
        colorRow.push(`hsl(${color}, ${SATURATION}%, ${luminosity}%)`)
      }
      rainbowColors.push(colorRow);
    }

    return rainbowColors
  }
}