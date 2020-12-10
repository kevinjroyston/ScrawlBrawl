import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {ColorPickerComponent} from '../colorpicker/colorpicker.component'
import {DrawingDirective} from '@shared/components/drawingdirective.component';
import {MatBottomSheet, MatBottomSheetConfig} from '@angular/material/bottom-sheet';
import {GalleryTool} from '@shared/components/gallerytool/gallerytool.component';
import Galleries from '@core/models/gallerytypes';

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
    _galleryType:string;
    @Input() drawingPrompt: DrawingPromptMetadata;
    @Input() showColorSelector: boolean = true;
    @Input() showEraser: boolean = true;
    @Input() showBrushSizeSelector: boolean = true;
    @Input() set galleryType(value: string) { this._galleryType = value; if (!this.drawingPrompt){this.createDrawingPrompt()} }
             get galleryType(): string { return this._galleryType}

    @Input() gameId: string;


    @ViewChild(DrawingDirective) drawingDirective;
    @ViewChild('galleryTool') galleryTool: GalleryTool;

    onChange;
    selectedColor: string;
    selectedBrushSize: number = 10;
    drawingOptionsCollapse: boolean = false;
    lastImageChange: string = "";
    showGallery: boolean = true;
    eraserMode: boolean = false; // Todo make brush-mode enum + group brush options into one object

    constructor(private _colorPicker: MatBottomSheet) {
    }

    createDrawingPrompt(){
      let gallery = Galleries.galleryFromType(this.galleryType);
      this.drawingPrompt = {
        colorList: null,
        widthInPx: gallery.imageWidth,
        heightInPx: gallery.imageWidth,
        premadeDrawing: "",
        canvasBackground: "",
        localStorageId:"",
        galleryType: this.galleryType,
        gameId: this.gameId
      }
     
    }


    ngOnInit() {
    }

    ngOnDestroy() {
    }

    ngAfterViewInit(){
        if (this.drawingPrompt) {
            this.gameId = this.drawingPrompt.gameId;
            this.galleryType = this.drawingPrompt.galleryType;
        }
        
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

    onDrawingChange(event){
        this.galleryTool.onDrawingChange(event);
        this.onChange(event)         
    }

    @HostListener('mousedown', ['$event'])
    @HostListener('touchstart', ['$event'])
    onmousedown(event) { 
    }


    @HostListener('mouseup', ['$event'])
    @HostListener('touchend', ['$event'])
    onmouseup(event) {
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
    galleryType: string;
    gameId: string;  /* should we be getting this from fetch-data? */
}
