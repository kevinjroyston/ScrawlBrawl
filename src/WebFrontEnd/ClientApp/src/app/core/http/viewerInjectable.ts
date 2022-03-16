
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
          }
    }

    
    createDIV(containerId) {
        // Use Renderer2 to create the div element
        this.viewerDiv = this.renderer.createElement('div');
        // Set the id of the div
        this.renderer.setProperty(this.viewerDiv, 'id', containerId);
        this.renderer.appendChild(document.body, this.viewerDiv);
    }

   
    InitializeViewer(containerId):void{
        this.createDIV(containerId);
     
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
    var progressBarFull = document.querySelector("#unity-load-viewer");
    var fullscreenButton = document.querySelector("#unity-fullscreen-button");
    var warningBanner = document.querySelector("#unity-warning");


    var el = container;
    this.renderer.setStyle(canvas,'width',el.clientWidth+'px');
    this.renderer.setStyle(canvas,'height',Math.round((el.clientWidth)*54/96)+'px');

    createUnityInstance(canvas, config, (progress) => {
      if (progressBarFull) {
            this.renderer.setStyle(progressBarFull,'value',100 * progress);
          }
    }).then((unityInstance) => {
      this.renderer.setStyle(loadingBar,'display','none');
      
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
          
    }

     
}