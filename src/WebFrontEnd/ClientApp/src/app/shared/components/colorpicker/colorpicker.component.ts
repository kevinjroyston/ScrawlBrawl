import { Component, OnInit, Input, Inject } from '@angular/core';
import {MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA} from '@angular/material/bottom-sheet';
import PastColorsService from './pastColors'
import ColorPickerService from './colorPicker'

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
  pastColorsService: PastColorsService
  pastColors: string[]
  grayScaleColors : string[]
  rainbowColors: string[][]

  constructor(
    private _colorPickerRef: MatBottomSheetRef<ColorPickerComponent>, 
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: ColorPickerData
  ) {
    let colorPickerService = new ColorPickerService();
    this.pastColorsService = new PastColorsService();
    this.pastColors = this.pastColorsService.retrievePastColors();
    this.grayScaleColors = colorPickerService.generateGrayScale();
    this.rainbowColors = colorPickerService.generateRainbow();
  }

  ngOnInit() {}

  pickColor(color: string) : void {
    this.pastColorsService.addColor(color);
    this.data.handleColorChange(color);
    this._colorPickerRef.dismiss();
  }
}