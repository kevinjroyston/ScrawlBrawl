import { Component, ViewEncapsulation, ElementRef, AfterViewInit, Input, Output, forwardRef } from '@angular/core';
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
    encapsulation: ViewEncapsulation.None,
})
export class Slider implements ControlValueAccessor {
  @Input() sliderParameters: SliderPromptMetadata;

  el: ElementRef;

  sliderSelection: string="";
  theRangeHighlights: RangeHighlightsType[];

  sliderValueToText(sp: SliderPromptMetadata, value): string {
    return "test";
  }
  updateSetting(sp: SliderPromptMetadata, event) {
    sp.value = event.value;
//    sp.currentTextValue = this.sliderValueToText(sp,event.value);
}
/*
  formatLabel = (sp: SliderPromptMetadata) => {
      return (value: number) => {
        if (!sp) return "";
        return this.sliderValueToText(sp,value);
      }
    }
*/    
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
    if (isDisabled) console.log("slider disabled");
  }

  constructor(element: ElementRef) {
    console.log("build slider");
    this.el = element.nativeElement;

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
    this.sliderSelection = this.sliderParameters.range?"before":"none";
    if (this.sliderParameters && this.sliderParameters.rangeHighlights) {
      this.theRangeHighlights = this.sliderParameters.rangeHighlights;
    }
  }

  ngAfterViewInit() {
    let myDivs = document.getElementsByClassName('slider-disabled');
    for (var i = 0; i < myDivs.length; i++) {
      myDivs[i].classList.remove('slider-disabled'); // we want the slider to behave disabled, but not look disabled
    }
  }

  onChange = function (s) { console.log("slider onchange called before initialization"); };


}

interface RangeHighlightsType
{
    start: number;
    end: number;
    class: string; 
}

interface SliderPromptMetadata {
  min: number;
  max: number;
  value: number[];
  ticks: number[];
  range: boolean;
  enabled: boolean;
  showTooltip: string;
  ticksLabels: string[];
  rangeHighlights: RangeHighlightsType[];
}
