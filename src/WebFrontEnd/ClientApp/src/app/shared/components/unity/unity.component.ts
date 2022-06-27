import { Component, OnInit, ElementRef, Input, DebugElement, HostListener } from '@angular/core';
import { UnityViewer } from '@core/http/viewerInjectable';
import { Inject } from '@angular/core';
import { ResizableModule , ResizeEvent } from 'angular-resizable-element';


//Used to create a single instance of the unity viewer

@Component({
  selector: 'unity',
  templateUrl: './unity.component.html',
  styleUrls: ['./unity.component.css']
})
export class UnityComponent implements OnInit {

  gameInstance: any;
  
  unityViewer: UnityViewer;
  progress = 0;
  isReady = true;
  element;

  constructor(@Inject(UnityViewer) private theViewer: UnityViewer,  element: ElementRef) {
    this.unityViewer = theViewer;
    this.element = element.nativeElement;  
  }

  ngOnInit(): void {

 /*   this.style = {
      //      position: 'fixed',
      //      left: '${event.rectangle.left}px',
      //      top: '${event.rectangle.top}px',
            width: '100%',
            height: '540px'
    };  */
    this.unityViewer.InitializeViewer('gameContainer');
/*    
    const loader = (window as any).UnityLoader;

    this.gameInstance = loader.instantiate(
    'gameContainer', 
    '/viewer/Build/ScrawlBrawlWebViewer.json', {
    onProgress: (gameInstance: any, progress: number) => {
        this.progress = progress;
        if (progress === 1) {
          this.isReady = true;
        }
      }
    });
*/    
  }
  

/*
  public style: object = {};

  validate(event: ResizeEvent): boolean {
    const MIN_DIMENSIONS_PX: number = 50;
    if (
      event.rectangle.width &&
      event.rectangle.height &&
      (event.rectangle.width < MIN_DIMENSIONS_PX ||
        event.rectangle.height < MIN_DIMENSIONS_PX)
    ) {
      return false;
    }
    return true;
  }

  onResizeEnd(event: ResizeEvent): void {
    this.style = {
//      position: 'fixed',
//      left: '${event.rectangle.left}px',
//      top: '${event.rectangle.top}px',
//      width: '${event.rectangle.width}px',
      height: `${event.rectangle.height}px`
    };
    setTimeout(() => {
      this.unityViewer.FixBlurriness();  
    }, 100);
    
  }*/

}