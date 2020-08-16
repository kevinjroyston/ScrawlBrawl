import { Component, ViewEncapsulation, ElementRef, Input, Output, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

/* https://www.npmjs.com/package/ngx-bootstrap-slider   */

/*
    sliderElement: any;
    slider: any;
    initialOptions: any;
    initialStyle: any;
    set style(value: any);
    set value(value: number | any[]);
    valueChange: EventEmitter<any>;
    set min(value: number);
    set max(value: number);
    set step(value: number);
    set precision(value: number);
    set orientation(value: string);
    set range(value: boolean);
    set selection(value: string);
    set tooltip(value: string);
    set tooltipSplit(value: boolean);
    set tooltipPosition(value: string);
    set handle(value: string);
    set reversed(value: boolean);
    set rtl(value: boolean);
    set enabled(value: boolean);
    set naturalArrowKeys(value: boolean);
    set ticks(value: any[]);
    set ticksPositions(value: number[]);
    set ticksLabels(value: string[]);
    set ticksSnapBounds(value: number);
    set ticksTooltip(value: boolean);
    set scale(value: string);
    set labelledBy(value: string[]);
    set rangeHighlights(value: any[]);
    set formatter(value: Function);
    set lockToTicks(value: Function);
    slide: EventEmitter<any>;
    slideStart: EventEmitter<any>;
    slideStop: EventEmitter<any>;
    change: EventEmitter<any>;

 */

@Component({
    selector: 'slider',
    templateUrl: './slider.component.html',
    styleUrls: ['./slider.component.css'],
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        multi: true,
        useExisting: Slider
    }],
    encapsulation: ViewEncapsulation.Emulated
})
export class Slider implements ControlValueAccessor {
  @Input() sliderParameters: SliderPromptMetadata;

  
  sliderValueToText(sp: SliderPromptMetadata, value): string {
    return "";
/*
    if (sp.textValueList.length < 2) return "";
    let ind: number;
    let w: number = Math.floor((sp.maxValue - sp.minValue) / (sp.textValueList.length - 1));
    if (w < 1) return "";
    let bin: number = Math.floor((value+(w/2)) / w)
    return sp.textValueList[bin]
*/
  }
  updateSetting(sp: SliderPromptMetadata, event) {
    sp.value = event.value;
//    sp.currentTextValue = this.sliderValueToText(sp,event.value);
}
  formatLabel = (sp: SliderPromptMetadata) => {
      return (value: number) => {
        if (!sp) return "";
        return this.sliderValueToText(sp,value);
      }
    }
    
  writeValue(obj: any): void {
    if (obj == null) {  // remove the slider

    } else {

    }
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
    console.log("slider register onChange");
      // Default needs a delay due to buggy form implementation.
      setTimeout(() => fn(this.sliderParameters.value), 20)
  }
  registerOnTouched(fn: any): void {
  }
  setDisabledState?(isDisabled: boolean): void {
  }

  constructor(element: ElementRef) {
    console.log("build slider");
    //    this.element = element.nativeElement;
  }

    valueChange(val: any) {
        if (this.sliderParameters.range) {
            this.onChange(val);
        } else {
            this.onChange([val]);
        }
    }

  ngOnInit() {
    console.log("on init - slider");
      console.log(this.sliderParameters);


    /*
        this.createRotatorImages(this.sliderParameters.widthInPx, this.sliderParameters.heightInPx);
        for (let i = 0; i < this.sliderParameters.imageList.length; i++) {
          this.addRotatorImage(this.sliderParameters.imageList[i]);
        }
        this.index = 0;
        this.setImages(0);
    */
  }




  onChange = function (s) { console.log("slider onchange called before initialization"); };
  element;


}

interface SliderPromptMetadata {
  min: number;
  max: number;
  value: number[];
  ticks: number[];
  range: boolean;
  ticksLabels: string[];
}
