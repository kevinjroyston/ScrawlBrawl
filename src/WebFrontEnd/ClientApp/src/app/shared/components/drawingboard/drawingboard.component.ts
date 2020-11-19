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
        if (this.drawingPrompt && this.drawingPrompt.colorList && this.drawingPrompt.colorList.length > 0) {
            this.selectedColor = this.drawingPrompt.colorList[0];
        }
        console.log(this.selectedColor);
    }

    ngAfterViewInit() {
        // use this if you need to reference any data in drawing directive
        console.log(this.selectedColor)
        this.selectedColor = this.drawingDirective.defaultLineColor;
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
