import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {ColorPickerComponent} from '../colorpicker/colorpicker.component'
import {DrawingDirective,DrawingPromptMetadata} from '@shared/components/drawingdirective.component';
import {MatBottomSheet, MatBottomSheetConfig} from '@angular/material/bottom-sheet';
import { FixedAsset } from '@core/http/fixedassets';
import { Inject } from '@angular/core';
import Galleries from '@core/models/gallerytypes';

const MaxGallerySize:number=10;

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

    @Input() drawingPrompt: DrawingPromptMetadata;
    @Input() galleryPanelType: string; /* Favorites | Recent | Sample */

    @ViewChild("galleryPictures") galleryPictures: ElementRef;

    onChange;
    galleryType: string;
    gameId: string;
    gallery: GalleryDrawing[]=[];

    
    constructor(@Inject(FixedAsset) private fixedAsset: FixedAsset, private _colorPicker: MatBottomSheet) {
    }

    loadTheGallery(){
        if (this.galleryType) {
            if (this.galleryPanelType==Galleries.samples) {
                this.loadGallerySamples(); /* loading multiple copies just to fill out the list, delete the rest */
            } else {
                this.loadGalleryImages();
            }
        }

    }

    removeExistingGallery(){
        if (this.gallery.length > 0){
            this.gallery.length = 0;
        }
        while (this.galleryPictures.nativeElement.firstChild) {
            this.galleryPictures.nativeElement.removeChild(this.galleryPictures.nativeElement.firstChild);
        }
    }
    setGalleryType(galType){
       if (galType != this.galleryType){
            this.removeExistingGallery();
            this.galleryType = galType;
            this.loadTheGallery();
       }
    }

    ngAfterViewInit(){
        if (this.drawingPrompt) {
            this.gameId = this.drawingPrompt.gameId;
            this.galleryType = this.drawingPrompt.galleryType;
        }
        
        this.loadTheGallery();
    }

    loadGallerySamples(){
        this.fixedAsset.fetchFixedAsset(this.fixedAsset.determineGalleryURI(this.galleryType)).subscribe({
                next: (data) => {
                    if (data){
                        var rawGallery: GalleryDrawing[];
                        rawGallery =  JSON.parse(data);
                        rawGallery.forEach((galleryDrawing)=>{this.galleryDisplayImage(galleryDrawing.image)})
                    }
                },
            });
    }

    loadGalleryImages(){
        this.loadLocalGalleryImages(); /* in the future we will support loading from DB */
    }

    loadLocalGalleryImages(){
        /* currently loading from local storage, could load from DB in future */
        if (this.galleryType) {
            var storedGallery=localStorage.getItem('Gallery-'+this.galleryType+'-'+this.galleryPanelType);
            var rawGallery: GalleryDrawing[];

            if (storedGallery) {
                rawGallery =  JSON.parse(storedGallery);
            /* sort the list so our game images come first - do we care about this? */
                this.gallery =  rawGallery.sort((a,b)=>{
                     if (a.gameId==this.gameId){
                        return (b.gameId==this.gameId) ? 0 : -1;
                     };
                     if (b.gameId==this.gameId){
                        return 1;
                     };
                    return 0; 
                    })
                this.gallery.forEach((galleryDrawing)=>{this.galleryDisplayImage(galleryDrawing.image)})
            } 
        }
    }

   
    galleryCountForGame(gameId){
       var count:number=0;
       this.gallery.forEach(function (drawing){if (drawing.gameId==gameId){count++}})
       return count;
    }

    storeImageInGallery(imgStr){
        if (imgStr=='') {return}
        let alreadyInList:number=-1;
        this.gallery.forEach(function (drawing,index){if (drawing.image==imgStr){alreadyInList=index; return}})
        if (alreadyInList<0){
            /* if we are at max length for this game, then delete the first image that is not marked favorite */
            if (this.galleryCountForGame(this.gameId) >= MaxGallerySize)  {
                let delIndex:number = -1;
                let findId=this.gameId;
                this.gallery.forEach(function (drawing,index){if (drawing.gameId==findId){delIndex=index; return}})
                if (delIndex >= 0) {
                  this.gallery.splice(delIndex, 1);
                }
            }
            var drawing : GalleryDrawing = {gameId:this.gameId, image:imgStr }
            this.gallery.push(drawing);

            this.saveGalleryToLocalStorage();
            this.galleryDisplayImage(drawing.image);
        }
    }

    removeImageFromGallery(imgStr){
        if (imgStr=='') {return}
        let placeInList:number=-1;
        this.gallery.forEach(function (drawing,index){if (drawing.image==imgStr){placeInList=index; return}})
        if (placeInList>=0){
            this.gallery.splice(placeInList, 1);
            this.saveGalleryToLocalStorage();
        }
    }

    deleteImage(img){ /* expects an IMG element */
        if (img.src) { 
            this.removeImageFromGallery(img.src);
            img.parentNode.removeChild(img);
        }
    }


    saveGalleryToLocalStorage(){
        if (this.galleryType) {
            var data = JSON.stringify(this.gallery.filter(img => img.image.length > 0));  /* don't write out sample images */
            localStorage.setItem('Gallery-'+this.galleryType+'-'+this.galleryPanelType,data);
        }
    }
    saveImageAsFavorite(imgStr){
        if (this.galleryPanelType!=Galleries.favorites) { alert('Unable to save favorite'); return}
        this.storeImageInGallery(imgStr);
    }

    saveImageAsRecent(imgStr){
        if (this.galleryPanelType!=Galleries.recent) { alert('Unable to save recent'); return}
        this.storeImageInGallery(imgStr);
    }
    ngOnInit() {
    }

    ngOnDestroy() {
    }

    @HostListener('click', ['$event.target'])
      onclick(img) {
            if (img.src) { /* if we clicked on an image, move it to the drawing */
                this.drawingDirective.loadImageString(img.src);
                event.preventDefault;
            }
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

    galleryStartClick()  {
        setTimeout(()=>this.galleryPictures.nativeElement.animate( { scrollLeft: '-=150' }, 100),100);
    }
    galleryEndClick()  {
        setTimeout(()=>this.galleryPictures.nativeElement.animate( { scrollLeft: '+=150' }, 100),100);
    }

    loadMostRecent(){
        if (this.gallery.length > 0) {
            this.drawingDirective.loadImageString(this.gallery[this.gallery.length-1].image);
        }
    }

    loadRecentIfRequested(){
        if (this.galleryPanelType && this.drawingDirective) {
          if (this.galleryPanelType==Galleries.recent){
               this.loadMostRecent()        
            }
        }
    }

    galleryDisplayImage(imgStr) {
        var img=document.createElement('img');
        img.width=30;
        img.height=30;
        img.src=imgStr;
        this.galleryPictures.nativeElement.append(img); //"<img src='"+imgStr+"'>");
        /* example showed appending &nbsp; after last image... needed? */
    }

    putGalleryOnClipboard(){
        var data = JSON.stringify(this.gallery.filter(img => img.image.length > 0));  /* don't write out sample images */
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
    gameId: string;
    image: string;
}

