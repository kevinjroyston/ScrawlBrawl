import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {ColorPickerComponent} from '../colorpicker/colorpicker.component'
import {GalleryTool} from '@shared/components/gallerytool/gallerytool.component';
import {DrawingDirective,DrawingPromptMetadata} from '@shared/components/drawingdirective.component';
import {MatBottomSheet, MatBottomSheetConfig} from '@angular/material/bottom-sheet';
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
    private _galleryId:string;
    @Input() drawingOptions: DrawingPromptMetadata;
    @Input() showColorSelector: boolean = true;
    @Input() showEraser: boolean = true;
    @Input() showBrushSizeSelector: boolean = true;
    @Input() set galleryId(value: string) { this.setGalleryId(value) }
             get galleryId(): string { return this._galleryId}

    @Input() galleryEditor: boolean;

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

    private updateDrawingOptionsForGalleryId(id){
        let gallery = Galleries.galleryFromId(this.galleryId);
        if (!this.drawingOptions) { /* if we are in a stand alone gallery editor, we do not have a drawing prompt, create one */
            this.drawingOptions = {
                colorList: null,
                widthInPx: gallery.imageWidth,
                heightInPx: gallery.imageHeight,
                premadeDrawing: "",
                canvasBackground: "",
                galleryOptions:{    
                    galleryId: this.galleryId,
                    galleryAutoLoadMostRecent: false,
                }
            }
        }
        this.drawingOptions.widthInPx = gallery.imageWidth;
        this.drawingOptions.heightInPx = gallery.imageHeight;
        this.drawingOptions.canvasBackground = gallery.canvasBackground;
    }

    private setGalleryId(id){
        if (this._galleryId == id) { return }
        
        this._galleryId = id; 
        this.updateDrawingOptionsForGalleryId(id);

        if (this.galleryTool) {
            this.galleryTool.galleryId=id;
        }
    }

    ngOnInit() {
    }

    ngOnDestroy() {
    }

    ngAfterViewInit(){
        // If this is a prompt from the backend drawingOptions will be defined here.
        // If it is a gallery editor created on the front end, then drawingOptions is created above when the galleryId input is set
        if (this.drawingOptions && this.drawingOptions.galleryOptions) {
            this.galleryId = this.drawingOptions.galleryOptions.galleryId;
        }
        
        // If there is a required color list default to first color.
        if (this.drawingOptions && this.drawingOptions.colorList && this.drawingOptions.colorList.length > 0) {
            this.selectedColor = this.drawingOptions.colorList[0];
        }

        // If there is no required color list or if the defaultLineColor is in the color list, default to that.
        let tempColor = this.drawingDirective.defaultLineColor;
        if (!this.drawingOptions || !this.drawingOptions.colorList || this.drawingOptions.colorList.includes(tempColor)) {
            this.selectedColor = tempColor;
        }

    }

    onDrawingChange(event){
        if (this.galleryTool) {
            this.galleryTool.onDrawingChange(event);
        }
        if (this.onChange) {
            this.onChange(event)
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

