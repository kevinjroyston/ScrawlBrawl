import { Component, OnInit, ElementRef, Input, DebugElement } from '@angular/core';
import { UnityViewer } from '@core/http/viewerInjectable';
import { Inject,Renderer2 } from '@angular/core';
import { ResizableModule , ResizeEvent } from 'angular-resizable-element';


//Used to create a single instance of the unity viewer

//#region Methods defined in JS file. This is so stupid
declare function createUnityInstance(canvas, config, onProgress): any;
//#endregion


@Component({
  selector: 'unity',
  templateUrl: './unity.component.html',
  styleUrls: ['./unity.component.css']
})
export class UnityComponent implements OnInit {

  @Input() lobbyId: string;

  private _gameInstance: any;
  set gameInstance(val){ this._gameInstance = val; this.invokeLobby();}
  get gameInstance(): any { return this._gameInstance}

  unityViewer: UnityViewer;
  progress = 0;
  isReady = true;
  element;
  renderer: Renderer2;

  constructor(@Inject(UnityViewer) private theViewer: UnityViewer,  element: ElementRef, private theRenderer: Renderer2) {
    this.unityViewer = theViewer;
    this.element = element.nativeElement;
    this.renderer = theRenderer;
  }

  invokeLobby() {

  }

  ngOnInit(): void {

 /*   this.style = {
      //      position: 'fixed',
      //      left: '${event.rectangle.left}px',
      //      top: '${event.rectangle.top}px',
            width: '100%',
            height: '540px'
    };  */
    this.InitializeViewer();
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
  
  InitializeViewer():void{ 
    var buildUrl = "viewer/Build";
    var loaderUrl = buildUrl + "/WebGl.loader.js";
    var config = {
      dataUrl: buildUrl + "/WebGl.data",
      frameworkUrl: buildUrl + "/WebGl.framework.js",
      codeUrl: buildUrl + "/WebGl.wasm",
      streamingAssetsUrl: "StreamingAssets",
      companyName: "KevinRoyston",
      productName: "ScrawlBrawl",
      productVersion: "1.0",
    };

    var container = document.querySelector("#unity-container");
    var canvas = document.querySelector("#unity-canvas");
    var loadingBar = document.querySelector("#unity-loading-bar");
    var progressBarFull = document.querySelector("#unity-progress-bar-full");
    var fullscreenButton = document.querySelector("#unity-fullscreen-button");
    var warningBanner = document.querySelector("#unity-warning");

    this.renderer.setStyle(canvas,'width','480px');
    this.renderer.setStyle(canvas,'height','270px');

    createUnityInstance(canvas, config, (progress) => {
      //progressBarFull.style.width = 100 * progress + "%";
    }).then((unityInstance) => {
      //loadingBar.style.display = "none";
      
      this.gameInstance = unityInstance;

      
      unityInstance.SendMessage("JavascriptConnector", "ConnectToLobby", this.lobbyId);
      
      //const urlParams = new URLSearchParams(window.location.search);
      //unityInstance.SendMessage("JavascriptConnector", "ConnectToLobby", urlParams.get('lobby'));
      /*fullscreenButton.onclick = () => {
        unityInstance.SetFullscreen(1);
      };*/
    }).catch((message) => {
      alert(message);
    });
    

    //loadingBar.style.display = "block";

    /*var script = document.createElement("script");
    script.src = loaderUrl;
    script.onload = () => {
      createUnityInstance(canvas, config, (progress) => {
        //progressBarFull.style.width = 100 * progress + "%";
      }).then((unityInstance) => {
        //loadingBar.style.display = "none";
        
        this.gameInstance = unityInstance;
        
        //const urlParams = new URLSearchParams(window.location.search);
        //unityInstance.SendMessage("JavascriptConnector", "ConnectToLobby", urlParams.get('lobby'));
        /*fullscreenButton.onclick = () => {
          unityInstance.SetFullscreen(1);
        };
      }).catch((message) => {
        alert(message);
      });
    };*/
    //document.body.appendChild(script);
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