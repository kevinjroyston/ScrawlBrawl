import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {ColorPickerComponent} from '../colorpicker/colorpicker.component'
import {DrawingDirective,DrawingPromptMetadata} from '@shared/components/drawingdirective.component';
import {MatBottomSheet, MatBottomSheetConfig} from '@angular/material/bottom-sheet';
import { FixedAsset } from '@core/http/fixedassets';
import { Inject } from '@angular/core';
import Galleries from '@core/models/gallerytypes';

@Component({
    selector: 'gallerypanel',
    templateUrl: './gallerypanel.component.html',
    styleUrls: ['./gallerypanel.component.scss'],
    encapsulation: ViewEncapsulation.Emulated
})

export class GalleryPanel implements ControlValueAccessor, AfterViewInit {

    private _drawingDirective: DrawingDirective;
    @Input() set drawingDirective(value: DrawingDirective) { this._drawingDirective = value; this.loadRecentIfRequested() }
             get drawingDirective(): DrawingDirective { return this._drawingDirective}

    @Input() drawingOptions: DrawingPromptMetadata;
    @Input() galleryPanelType: string; /* Favorites | Recent | Sample */

    @ViewChild("galleryPictures") galleryPictures: ElementRef;

    onChange;
    galleryId : string;
    gallery: GalleryDrawing[]=[];
    galleryType: Galleries.GalleryType;

    
    constructor(@Inject(FixedAsset) private fixedAsset: FixedAsset, private _colorPicker: MatBottomSheet) {
    }

    loadTheGallery(){
        if (this.galleryId) {
            if (this.galleryPanelType==Galleries.samples) {
                this.fetchGallerySamples(); 
            } else {
                this.loadGalleryImages();
            }
        }
    }

    saveTheGallery(){
        this.saveGalleryToLocalStorage(); /* in the future we will support saving to DB */
    }

    loadGalleryImages(){
        this.loadLocalGalleryImages(); /* in the future we will support loading from DB */
    }

    setGalleryId(galId){
       if ((galId) && (galId != this.galleryId)){
            this.clearExistingGallery();
            this.galleryId  = galId;
            this.galleryType=Galleries.galleryFromId(this.galleryId);
            this.loadTheGallery();
       }
    }

    ngAfterViewInit(){
        if (this.drawingOptions && this.drawingOptions.galleryOptions) {
            this.setGalleryId(this.drawingOptions.galleryOptions.galleryId);
        }
    }

    loadRecentIfRequested(){
        // if the gallery option galleryAutoLoadMostRecent is true, then take the last "recent" image and put it on the canvas
        if (this.galleryPanelType && this.drawingDirective) {
          if ((this.drawingDirective.galleryAutoLoadMostRecent) && (this.galleryPanelType==Galleries.recent) && (this.gallery.length > 0)) {
              this.drawingDirective.loadImageString(this.gallery[this.gallery.length-1].image);
          }
        }
    }



    maxGallerySize():number {
        // find the max based on gallerytype and panel type
        if (this.galleryPanelType!=Galleries.recent) {
          return this.galleryType.maxLocalFavorites
        } else {
          return this.galleryType.maxLocalRecent
        }
    }

    storeImageInGallery(imgStr){
        if ((imgStr=='') || (this.galleryId==Galleries.samples)) {return}  // can't write to samples
        let alreadyInList:boolean=false;
        this.gallery.forEach(function (drawing,index){if (drawing.image==imgStr){alreadyInList=true; return}})
        if (!alreadyInList){
            /* if we are at max length for this gallery type, error if favorites, delete the first (oldest) image if recent */
            if (this.gallery.length >= this.maxGallerySize())  {
                if (this.galleryPanelType==Galleries.favorites) {
                    alert("You can only save "+this.maxGallerySize()+" favorites.");
                    return;
                }
                this.gallery.splice(0, 1);
            }
            var drawing : GalleryDrawing = { image:imgStr }
            this.gallery.push(drawing);

            this.saveTheGallery();
            this.galleryDisplayImage(drawing.image);
        }
    }

    removeImageFromGallery(imgStr){
        if (imgStr!='') {
            let placeInList:number=-1;
            this.gallery.forEach(function (drawing,index){if (drawing.image==imgStr){placeInList=index; return}})

            if (placeInList>=0){
                this.gallery.splice(placeInList, 1);
                this.saveTheGallery();
            }
        }
    }

    
    ngOnInit() {
    }
    
    ngOnDestroy() {
    }

    writeValue(obj: any): void {
    }

    registerOnChange(fn: any): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: any): void {
    }

    /************ the following routines deal with the images in the GalleryPictured div  *************/

    deleteImage(img){ /* not currently be used.  pass in an <img> element and it will find it and delete it from gallery and the display */
        if (img.src) { 
            this.removeImageFromGallery(img.src);
            img.parentNode.removeChild(img);
        }
    }

    galleryDisplayImage(imgStr) { /* add an image to the galleryPictures div */
        var img=document.createElement('img');
        img.width=30;
        img.height=30;
        img.src=imgStr;
        this.galleryPictures.nativeElement.append(img);
    }

    @HostListener('click', ['$event.target'])
      onclick(img) {
            if (img.src) { /* if we clicked on an image, move it to the drawing */
                this.drawingDirective.loadImageString(img.src);
                event.preventDefault;
            }
        }

    clearExistingGallery(){
        if (this.gallery.length > 0){
            this.gallery.length = 0;
        }
        while (this.galleryPictures.nativeElement.firstChild) {
            this.galleryPictures.nativeElement.removeChild(this.galleryPictures.nativeElement.firstChild);
        }
    }


    /************ Fetch Samples from the fixed assets ***********/
    fetchGallerySamples(){
        this.fixedAsset.fetchFixedAsset(this.fixedAsset.determineGalleryURI(this.galleryId)).subscribe({
                next: (data) => {
                    if (data){
                        this.gallery  =  JSON.parse(data);
                        this.gallery.forEach((galleryDrawing)=>{this.galleryDisplayImage(galleryDrawing.image)})
                    }
                },
            });
    }

    /************ Local storage routines ***********/
    localStorageGalleryName():string{
       return 'Gallery-'+this.galleryId+'-'+this.galleryPanelType;
    }

    loadLocalGalleryImages(){
        if (this.galleryId) {
            var storedGallery=localStorage.getItem(this.localStorageGalleryName());

            if (storedGallery) {
                this.gallery =  JSON.parse(storedGallery);
                this.gallery.forEach((galleryDrawing)=>{this.galleryDisplayImage(galleryDrawing.image)})
            } 
        }
    }

    saveGalleryToLocalStorage(){
        if (this.galleryId) {
            var data = JSON.stringify(this.gallery); 
            localStorage.setItem(this.localStorageGalleryName(),data);
        }
    }

    /*********** clipboard functions for our testing purposes - these buttons are hidden on production server  */

    putGalleryOnClipboard(){
        var data = JSON.stringify(this.gallery.filter(img => img.image.length > 0));  /* don't write out sample images ***********/
        navigator.clipboard.writeText(data).then().catch(e => console.error(e));
        alert("Your gallery is on the clipboard.");
    }


    addClipboardToGallery(){
        /* note firefox needs: dom.events.asyncClipboard.dataTransfer   set to true in about:config */
        navigator.clipboard.readText().then(text => {
            var rawGallery: GalleryDrawing[];
                rawGallery =  JSON.parse(text);
                if ((rawGallery) && (rawGallery.length > 0)) {
                    rawGallery.forEach((galleryDrawing)=>{this.storeImageInGallery(galleryDrawing.image)});
                }
        }).catch(err => {
            console.log('Something went wrong', err);
        });
    }
    
}

interface GalleryDrawing {
    image: string;
}

