import { Component, Input, Output, EventEmitter } from '@angular/core';
import Galleries from '@core/models/gallerytypes';

@Component({
  selector: 'gallerypicture',
  templateUrl: './gallerypicture.component.html',
  styleUrls: ['./gallerypicture.component.scss']
})
export class GalleryPicture {
  @Input() drawing: Galleries.GalleryDrawing;
  @Input() galleryPanelType: Galleries.GalleryPanelType;
  @Output() deleteDrawing: EventEmitter<any> = new EventEmitter();

  constructor() { 
    
  }

  public isDeletable = () => {
    return this.galleryPanelType !== Galleries.GalleryPanelType.SAMPLES;
  }

  public deleteImage = () => {
    this.deleteDrawing.emit();
  }
}
