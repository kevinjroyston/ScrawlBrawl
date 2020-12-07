import { Component, OnInit, Input, Inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA} from '@angular/material/bottom-sheet';
import PastColorsService from './pastColors'
import ColorPickerService from './colorPicker'

interface ColorPickerData {
  handleColorChange: (color: string) => void
}

@Component({
  selector: 'app-colorpicker',
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    multi: true,
    useExisting: ColorPickerComponent
  }],
  templateUrl: './colorpicker.component.html',
  styleUrls: ['./colorpicker.component.scss']
})
export class ColorPickerComponent implements ControlValueAccessor, OnInit 
{
  @Input() isColorsSpecified : boolean = false
  pastColors: string[]
  grayScaleColors : string[]
  rainbowColors: string[][]
  onChange = (color)=> {console.log('no change defined')};

  constructor(
    private _colorPickerRef: MatBottomSheetRef<ColorPickerComponent>, 
    private colorPickerService : ColorPickerService,
    private pastColorsService : PastColorsService,
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: ColorPickerData
  ) {
    this.pastColors = this.pastColorsService.retrievePastColors();
    this.grayScaleColors = this.colorPickerService.generateGrayScale();
    this.rainbowColors = this.colorPickerService.generateRainbow();
  }

  ngOnInit() {}

  pickColor(color: string) : void {
    this.pastColorsService.addColor(color);
    this.data.handleColorChange(color);
    this.onChange(color);
    this._colorPickerRef.dismiss();
  }

  writeValue(obj: any): void {
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
  }

}