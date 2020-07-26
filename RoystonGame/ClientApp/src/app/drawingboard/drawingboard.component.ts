import { Component, ViewEncapsulation, Input, Output, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
    selector: 'drawingboard',
    templateUrl: './drawingboard.component.html',
    styleUrls: ['./drawingboard.component.css'],
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        multi: true,
        useExisting: DrawingBoard
    }],
    encapsulation: ViewEncapsulation.Emulated
})
export class DrawingBoard implements ControlValueAccessor {
    @Input() drawingPrompt: DrawingPromptMetadata;
    @Input() showColorSelector: boolean = true;
    @Input() showEraser: boolean = true;
    @Input() showBrushSizeSelector: boolean = true;

    onChange;
    selectedColor: string;
    selectedBrushSize: number = 15;
    drawingOptionsCollapse: boolean = false;
    eraserMode: boolean = false; // Todo make brush-mode enum + group brush options into one object

    ngOnInit() {
        if (this.drawingPrompt && this.drawingPrompt.colorList && this.drawingPrompt.colorList.length > 0) {
            this.selectedColor = this.drawingPrompt.colorList[0];
        }
    }

    writeValue(obj: any): void {
    }
    registerOnChange(fn: any): void {
        this.onChange = fn;
    }
    registerOnTouched(fn: any): void {
    }
    setDisabledState?(isDisabled: boolean): void {
    }
}

interface DrawingPromptMetadata {
    colorList: string[];
    widthInPx: number;
    heightInPx: number;
    premadeDrawing: string;
    canvasBackground: string;
}
