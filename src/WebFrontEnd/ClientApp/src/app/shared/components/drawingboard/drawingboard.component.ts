import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {ColorPickerComponent} from '../colorpicker/colorpicker.component'
import {DrawingDirective} from '@shared/components/drawingdirective.component';
import {MatBottomSheet, MatBottomSheetConfig} from '@angular/material/bottom-sheet';

@Component({
    selector: 'drawingboard',
    templateUrl: './drawingboard.component.html',
    styleUrls: ['./drawingboard.component.scss'],
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        multi: true,
        useExisting: DrawingBoard
    }],
    encapsulation: ViewEncapsulation.Emulated
})

export class DrawingBoard implements ControlValueAccessor, AfterViewInit {
    @Input() drawingPrompt: DrawingPromptMetadata;
    @Input() showColorSelector: boolean = true;
    @Input() showEraser: boolean = true;
    @Input() showBrushSizeSelector: boolean = true;
    @ViewChild(DrawingDirective) drawingDirective;

    onChange;
    selectedColor: string;
    selectedBrushSize: number = 10;
    drawingOptionsCollapse: boolean = false;
    eraserMode: boolean = false; // Todo make brush-mode enum + group brush options into one object

    constructor(private _colorPicker: MatBottomSheet) {}

    ngOnInit() {
    }

    ngAfterViewInit() {
        console.log(this.selectedColor)
        
        // If there is a required color list default to first color.
        if (this.drawingPrompt && this.drawingPrompt.colorList && this.drawingPrompt.colorList.length > 0) {
            this.selectedColor = this.drawingPrompt.colorList[0];
        }

        // If there is no required color list or if the defaultLineColor is in the color list, default to that.
        let tempColor = this.drawingDirective.defaultLineColor;
        if (!this.drawingPrompt || !this.drawingPrompt.colorList || this.drawingPrompt.colorList.includes(tempColor))
        {
            this.selectedColor = tempColor;
        }
    }

    onPerformUndo(): void {
        
        alert('undo')
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

    handleColorChange = (color: string) => {
        this.selectedColor = color
    }

    openColorPicker = (event: MouseEvent): void => {
        event.preventDefault();
        this.eraserMode = false
        const bottomConfig = new MatBottomSheetConfig();
        bottomConfig.data = {
            handleColorChange: (color: string) => this.handleColorChange(color),
            panelClass: 'sb-colorpicker-dialog'
        }
        this._colorPicker.open(ColorPickerComponent, bottomConfig)
    }
}

interface DrawingPromptMetadata {
    colorList: string[];
    widthInPx: number;
    heightInPx: number;
    premadeDrawing: string;
    canvasBackground: string;
    localStorageId: string;
}
