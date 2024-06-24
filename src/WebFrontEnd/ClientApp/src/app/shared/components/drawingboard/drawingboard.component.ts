import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {ColorPickerComponent} from '../colorpicker/colorpicker.component'
import {GalleryTool} from '@shared/components/gallery/gallerytool/gallerytool.component';
import {DrawingDirective,DrawingPromptMetadata,DrawingModes,DrawingUrgencies} from '@shared/components/drawingdirective.component';
import {MatBottomSheet, MatBottomSheetConfig} from '@angular/material/bottom-sheet';
import Galleries from '@core/models/gallerytypes';
import { EventManager } from '@angular/platform-browser';
import { GalleryService } from '@core/services/gallery.service';
import PastColorsService from '../colorpicker/pastColors';
import { Router } from '@angular/router';
import { ThemeService } from '@core/services/theme.service' 

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
    drawingModes = DrawingModes; /* so html can see it */

    private _drawingType:string;
    public galleryRecentDrawing:string; // if this is set, drawing canvas will use this as default.
    @Input() showColorSelector: boolean = true;
    @Input() showEraser: boolean = true;
    @Input() showBrushSizeSelector: boolean = true;
    @Input() drawingOptions:DrawingPromptMetadata;
    @Input() userPromptId:string="";
    @Input() drawingUrgency:DrawingUrgencies = DrawingUrgencies.None;
    @Input() set drawingType(value: string) { this.setDrawingType(value) }
             get drawingType(): string { return this._drawingType}

    @Input() galleryEditor: boolean;

    @ViewChild(DrawingDirective) drawingDirective;

    drawingMode : DrawingModes = DrawingModes.Draw;
    DrawingUrgencies = DrawingUrgencies; /* make the enum visible to the html template */
    drawingHeight;
    drawingWidth;
    onChange;
    selectedColor: string;
    previousColor: string="";
    selectedBrushSize: number = 10;
    drawingOptionsCollapse: boolean = false;
    lastImageChange: string = "";
    showGallery: boolean = true;

    constructor(private _colorPicker: MatBottomSheet, private _gallery: MatBottomSheet, private galleryService: GalleryService,
        private router:Router, private ThemeService: ThemeService ) {
            router.events.subscribe((val) =>{
                this.storeWorkInProgress();
            } )
    }

//    this.element.tabIndex = 1;

    @HostListener('keyup',['$event'])
    onKeyUp(event:KeyboardEvent){
        switch (event.key) {

        case 'X':
        case 'x':
                if (this.previousColor!="") this.handleColorChange(this.previousColor);
                break;
        case 'Z':
        case 'z':
                if (event.ctrlKey && !event.shiftKey) this.drawingDirective.onPerformUndo();
                if (event.ctrlKey && event.shiftKey) this.drawingDirective.onPerformRedo();
                break;
        case 'Y':
        case 'y':
                if (event.ctrlKey) this.drawingDirective.onPerformRedo();
                break;
        case 'B':
        case 'b':
                this.drawingMode=DrawingModes.Draw;
                break;
        case 'E':
        case 'e':
                this.drawingMode=DrawingModes.Erase;
                break;
        case 'F':
        case 'f':
                this.drawingMode=DrawingModes.FloodFill;
                break;
        case '[':
        case '{':
                if (this.selectedBrushSize > 2) this.selectedBrushSize--;
                break;
        case ']':
        case '}':
                if (this.selectedBrushSize < 40) this.selectedBrushSize++;
                break;
        
        }
    }

    private updateDrawingOptionsForDrawingType(typ){
        let gallery = Galleries.galleryFromDrawingType(typ);

        if (!this.drawingOptions || this.galleryEditor==true) { /* if we are in a stand alone gallery editor, we do not have a drawing prompt, create one */
            this.drawingOptions = {
                drawingType: typ,
                colorList: null,
                premadeDrawing: "",
                canvasBackground: "",
                saveWithBackground: false,
                galleryOptions:{    
                    galleryAutoLoadMostRecent: true,
                }
            }
        }
        this.galleryRecentDrawing = this.drawingOptions?.galleryOptions?.galleryAutoLoadMostRecent ? this.galleryService.GetMostRecentDrawing(typ) : null;
        if (localStorage.getItem("WIP-ID") == this.userPromptId) {
                this.galleryRecentDrawing = localStorage.getItem("WIP-Image");
                localStorage.setItem("WIP-Image","");
        }

        this.drawingWidth = gallery.imageWidth;
        this.drawingHeight = gallery.imageHeight;
        if (this.galleryEditor || !this.drawingOptions.canvasBackground) {  // use the gallery background, unless the drawing prompt gave us one
            this.drawingOptions.canvasBackground = gallery.canvasBackground;
        }
    } 

    private setDrawingType(typ){
        if (this._drawingType == typ) { return }
        
        
        // Any time drawing type is swapped, try and store a recent drawing.
        // Do this BEFORE we update the drawing type.
        if (this.drawingOptions && this.drawingOptions.galleryOptions){
            this.storeMostRecentDrawing(true);
        }

        this._drawingType = typ; 
        this.updateDrawingOptionsForDrawingType(typ);
        if (this.drawingDirective) this.drawingDirective.handleClearUndo();
    }

    protected saveToFavorites () {
        this.galleryService.AddImageToGallery(false, this.drawingType,Galleries.GalleryPanelType.FAVORITES,this.lastDrawingChange);
    }

    ngOnInit() {
        // If this is a prompt from the backend drawingOptions will be defined here.
        // If it is a gallery editor created on the front end, then drawingOptions is created above when the drawingType input is set
        if (this.drawingOptions) {
            this.drawingType = this.drawingOptions.drawingType;
        }
        
        // If there is a required color list default to first color.
        if (this.drawingOptions && this.drawingOptions.colorList && this.drawingOptions.colorList.length > 0) {
            this.selectedColor = this.drawingOptions.colorList[0];
        }

        // If there is no required color list or if the previously used color is in the color list, default to that.
        let pastColorsService = new PastColorsService();
        var tempColor = pastColorsService.getLastColor() || "rgb(0,0,0)";
        if (!this.drawingOptions || !this.drawingOptions.colorList || this.drawingOptions.colorList.includes(tempColor)) {
            this.selectedColor = tempColor;
        }

        this.previousColor = this.selectedColor;
    }

    ngOnDestroy() {
        this.storeMostRecentDrawing(true);
    }

    ngAfterViewInit(){
        

    }

    @HostListener('window:beforeunload', ['$event'])
    unloadHandler(event: Event) {
        this.storeWorkInProgress();
    }
    storeWorkInProgress(){
        if (this.lastDrawingChange != "" && this.lastDrawingChange != null && this.userPromptId!="") {
           localStorage.setItem("WIP-ID",this.userPromptId) 
           localStorage.setItem("WIP-Image", this.lastDrawingChange);
        }

    }
    private lastDrawingChange:string;
    storeMostRecentDrawing(onDestroy = false){
        if (this.drawingOptions.galleryOptions && this.lastDrawingChange != "" && this.lastDrawingChange != null) {
            this.galleryService.AddImageToGallery(onDestroy, this.drawingType,Galleries.GalleryPanelType.RECENT,this.lastDrawingChange);
            this.lastDrawingChange = "";
        }
    }
    onDrawingChange(event){
        this.lastDrawingChange = event;
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
        this.previousColor = this.selectedColor;
        this.selectedColor = color;

        document.body.classList.remove('theme-visible-drawing');

        if (this.ThemeService.isDarkMode()) {
            if (color=='rgb(63.75, 63.75, 63.75)') {
                document.body.classList.add('theme-visible-drawing');
            }
        } else
        {
            if (color=="rgb(255, 255, 255)" ) {
                document.body.classList.add('theme-visible-drawing');
            }
        }
        this.switchToDrawIfErase();
    }

    switchToDrawIfErase(){
        if (this.drawingMode == DrawingModes.Erase)
        {
            this.drawingMode = DrawingModes.Draw;
        }
    }

    openColorPicker = (event: MouseEvent): void => {
        event.preventDefault();
        this.switchToDrawIfErase();
        const bottomConfig = new MatBottomSheetConfig();
        bottomConfig.data = {
            handleColorChange: (color: string) => this.handleColorChange(color),
        }
        bottomConfig.panelClass ='sb-colorpicker-dialog';
        this._colorPicker.open(ColorPickerComponent, bottomConfig)
    }

    openGallery = (event: MouseEvent) : void => {
        event.preventDefault();
        const bottomConfig = new MatBottomSheetConfig();
        bottomConfig.data = {
            drawingOptions: this.drawingOptions,
            galleryEditor: this.galleryEditor,
            drawingDirective: this.drawingDirective
        }
        this._gallery.open(GalleryTool, bottomConfig);
    }
}

