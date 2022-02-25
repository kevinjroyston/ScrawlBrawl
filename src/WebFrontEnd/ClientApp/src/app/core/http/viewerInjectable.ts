
import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';
import { OnInit } from '@angular/core';

//Used to create a single instance of the unity viewer

//#region Methods defined in JS file. This is so stupid
declare function createUnityInstance(canvas, config, onProgress): any;
//#endregion

@Injectable({
    providedIn: 'root'
})
export class UnityViewer{

    gameInstance: any;
    containerId: string="";
    viewerDiv: Element = null;
    private lobbyId = null;
    private currentLobbyId = null;
    public progress = 0;
    public isReady = false;
  
    private renderer: Renderer2;
    constructor (rendererFactory: RendererFactory2) {
        // Get an instance of Angular's Renderer2
        this.renderer = rendererFactory.createRenderer(null, null);
    }

   /* FixBlurriness(){
        var canvas = this.gameInstance.Module.canvas;
        var container = this.gameInstance.container;

//        container.style.width = '960px';//canvas.style.width;
//        container.style.height = '540px'; //canvas.style.height;            
//        container.style.width = canvas.style.width;
        var el = document.getElementById(this.containerId);
        if (Math.round((el.clientHeight-1)*96/54) <= el.clientWidth) {
          container.style.width = Math.round((el.clientHeight-1)*96/54)+'px';
          container.style.height = el.clientHeight-1+'px';
        }
        else
        {
          container.style.width = el.clientWidth+'px';
          container.style.height = Math.round((el.clientWidth)*54/96)+'px';
        }
    }*/

    showFullScreen(){
        if (this.gameInstance) {
            this.gameInstance.SetFullscreen(1);
        }
    }
    UpdateLobbyId(lobbyId) {
        this.lobbyId = lobbyId;
        if (lobbyId == this.currentLobbyId) return;
    
        if (this.gameInstance) {
            this.currentLobbyId = this.lobbyId;
            console.log('Sending lobby id to viewer');
            this.gameInstance.SendMessage("JavascriptConnector","ConnectToLobby",this.lobbyId);
        //   setTimeout(() => {
        //       this.FixBlurriness(); 
        //    }, 6000);
          }
    }

    
    createDIV(containerId) {
        // Use Renderer2 to create the div element
        this.viewerDiv = this.renderer.createElement('div');
        // Set the id of the div
        this.renderer.setProperty(this.viewerDiv, 'id', containerId);

        this.renderer.appendChild(document.body, this.viewerDiv);

    }

   /* InitializeViewer(containerId): void {
        this.containerId = containerId;
        if (!this.viewerDiv) {  // first call create a div and initialize the viewer
            this.createDIV();  
            const loader = (window as any).WebGlLoader;
  
            this.gameInstance = loader.instantiate(
                'viewerContainer', 
                '/viewer/Build/WebGL.Loader.js', {
            onProgress: (gameInstance: any, progress: number) => {
                  this.progress = progress;
                    if (progress === 1) {
                        this.isReady = true;
                        if (this.lobbyId) {
                            this.UpdateLobbyId(this.lobbyId);
                        }
                    }
                }
            });
        }   
    }*/

    InitializeViewer(containerId):void{
        this.createDIV(containerId);
     
      var buildUrl = "viewer/Build";
      var loaderUrl = buildUrl + "/WebGL.loader.js";
      var config = {
        dataUrl: buildUrl + "/WebGL.data",
        frameworkUrl: buildUrl + "/WebGL.framework.js",
        codeUrl: buildUrl + "/WebGL.wasm",
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
          
/*
      var script = document.createElement("script");
      script.src = loaderUrl;
      script.onload = () => {
        createUnityInstance(this.viewerDiv.canvas, config, (progress) => {
        }).then((unityInstance) => {
            this.gameInstance = unityInstance;
          
          const urlParams = new URLSearchParams(window.location.search);
          unityInstance.SendMessage("JavascriptConnector", "ConnectToLobby", urlParams.get('lobby'));
        }).catch((message) => {
          alert(message);
        });
      };
      
      document.body.appendChild(script);
        // Append the div to the body element
        this.renderer.appendChild(document.getElementById(containerId), this.viewerDiv);*/
    }

     
}