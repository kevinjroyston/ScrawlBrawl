import { Directive, ElementRef, HostListener, AfterViewInit, ViewChild, Input, Output, EventEmitter } from "@angular/core";
import { throttle } from "app/utils/throttle";
import PastColorsService from "./colorpicker/pastColors";
import * as drawingUtils from "app/utils/drawingutils";

const MaxUndoCount = 20;

export enum DrawingModes {
  Draw,
  Erase,
  FloodFill
}

@Directive({
  selector: "[appDrawing]",
})
export class DrawingDirective {
  ctx: any;
  @Input() lineColor: string;
  @Input() lineWidth: number;
  @Input() premadeDrawing: string;
  @Input() drawingMode: DrawingModes;
  @Input() galleryAutoLoadMostRecent: boolean;
  @Input() galleryEditor: boolean = false;
  @Output() drawingEmitter = new EventEmitter();
  defaultLineColor: string;
  element;
  private undoArray: string[] = [];
  private userIsDrawing: boolean;
  private lastX: number;
  private lastY: number;

  constructor(element: ElementRef) {
    console.log("Instantiating canvas");
    this.element = element.nativeElement;
    let pastColorsService = new PastColorsService();
    this.defaultLineColor = pastColorsService.getLastColor() || "rgb(0,0,0)";
    this.ctx = element.nativeElement.getContext("2d");
    this.userIsDrawing = false;
  }

  loadImageString(imgStr) {
    if (imgStr && this.ctx) {
      var img = new Image();
      var ctx = this.ctx;
      img.onload = function () {
        console.log("showing stored drawing");
        ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
        ctx.drawImage(img, 0, 0, ctx.canvas.width, ctx.canvas.height);
      };
      console.log("loading stored drawing");
      img.src = imgStr;
      this.onImageChange(imgStr);
    }
  }
  ngOnInit() {
    this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
  }

  ngAfterViewInit() {
    if (this.galleryAutoLoadMostRecent) {
      /* will be handled by gallerytool */
    } else if (this.premadeDrawing) {
      this.loadImageString(this.premadeDrawing);
    } else {
      this.onImageChange(null, false); /* so undo will work */
    }
  }

  emitImageChange(imgStr) {
    // write the data to the emitter
    this.drawingEmitter.emit(imgStr);
  }

  public clearDrawing() {
    this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
    this.onImageChange(null);
  }

  onImageChange(imgStr: string, emitChange = true) {
    if (this.ctx.canvas.width == 0) return;

    if (!imgStr) {
      imgStr = this.element.toDataURL();
    }
    if (emitChange) {
      this.emitImageChange(imgStr);
    }

    if (
      this.undoArray.length == 0 ||
      imgStr != this.undoArray[this.undoArray.length - 1]
    ) {
      // store it for an undo, if not already last thing on stack  #SB-169
      if (this.undoArray.length >= MaxUndoCount) {
        this.undoArray.shift();
      }
      this.undoArray.push(imgStr);
      console.log("saved undo " + this.undoArray.length);
    }
  }

  stopDrawing() {
    if (this.userIsDrawing) {
      this.userIsDrawing = false;
      this.onImageChange(null);
    }
  }

  handleClearUndo() {
    this.undoArray.length = 0;
    this.onImageChange(null, false); /* set base undo image */
  }

  onPerformUndo() {
    if (this.undoArray.length > 0) {
      if (this.undoArray.length > 1) {
        this.undoArray.pop();
      } // the first call to undo would have the value we just stored, we want to go one back
      var img = new Image();
      var ctx = this.ctx;
      img.onload = function () {
        console.log("Drawing undo to canvas");
        ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
        ctx.drawImage(img, 0, 0, ctx.canvas.width, ctx.canvas.height);
      };
      console.log("Loading undo drawing " + this.undoArray.length);
      let imgStr = this.undoArray[this.undoArray.length - 1];
      img.src = imgStr;
      this.emitImageChange(imgStr);
    }
  }
  @HostListener("mousedown", ["$event"])
  @HostListener("touchstart", ["$event"])
  onmousedown(event) {
    if (this.userIsDrawing) {
      console.log("down but already drawing");
      this.stopDrawing();
    } else {
      if (document.activeElement instanceof HTMLElement)
        // pull focus from any input so it hides the keyboard on mobile
        document.activeElement.blur();

      if (this.undoArray.length == 0) {
        this.onImageChange(null, false);
      } // initialize undo

      [this.lastX, this.lastY] = this.getCoords(event);

      if (this.drawingMode == DrawingModes.FloodFill) {
        let rgb = drawingUtils.convertColorToRGB(this.getColor()).split(",");
        let r = parseInt(rgb[0].substring(4)); // skip rgb(
        let g = parseInt(rgb[1]); // this is just g
        let b = parseInt(rgb[2]); // parseInt scraps trailing )
        if (drawingUtils.floodFill(this.ctx,Math.round(this.lastX),Math.round(this.lastY),r,g,b)) {
            this.onImageChange(null);
        }
      } else {
        // begins new line
        this.drawCircle(this.lastX, this.lastY);
        this.userIsDrawing = true;
      }
    }
  }

  @HostListener("mousemove", ["$event"])
  @HostListener("touchmove", ["$event"])
  onmousemove(event) {
    if (this.userIsDrawing) {
      event.preventDefault();
      // get current mouse position
      let [currentX, currentY] = this.getCoords(event);

      // draw
      this.drawLine(this.lastX, this.lastY, currentX, currentY);
      this.drawCircle(currentX, currentY);

      // set current coordinates to last one
      this.lastX = currentX;
      this.lastY = currentY;
    }
  }

  @HostListener("document:gesturestart")
  @HostListener("document:gestureend")
  ongesturestart() {
    console.log("gesture start");
    if (this.userIsDrawing) {
      this.stopDrawing();
      // we might want to call undo here? needs testing
    }
  }

  @HostListener("document:mouseup")
  @HostListener("document:touchend")
  @HostListener("mouseup")
  @HostListener("touchend")
  onmouseup() {
    if (this.userIsDrawing) {
      event.preventDefault(); // plr SB-169
      this.stopDrawing();
    }
  }

  @HostListener("window:scroll", ["$event"])
  @throttle(200)
  onScrollEvent($event) {
    console.log("scrolling");
    if (this.userIsDrawing) {
      this.stopDrawing();
    }
  }

  drawLine(lX, lY, cX, cY): void {
    if (this.drawingMode == DrawingModes.Erase)
      this.ctx.globalCompositeOperation = "destination-out";
    // line from
    this.ctx.beginPath();
    this.ctx.moveTo(lX, lY);
    // to
    this.ctx.lineTo(cX, cY);
    // color
    this.ctx.strokeStyle = this.getColor();
    //this.ctx.lineJoin = "round";
    this.ctx.lineWidth = this.lineWidth;
    // draw it
    this.ctx.stroke();
    this.ctx.globalCompositeOperation = "source-over";
  }

  getColor(): string {
    if (this.lineColor && this.lineColor != "") {
      return this.lineColor;
    }
    return this.defaultLineColor;
  }

  drawCircle(x, y): void {
    if (this.drawingMode == DrawingModes.Erase)
      this.ctx.globalCompositeOperation = "destination-out";
    this.ctx.beginPath();
    var radius = this.lineWidth / 2.1; // Arc radius
    var startAngle = 0; // Starting point on circle
    var endAngle = Math.PI * 2; // End point on circle

    this.ctx.moveTo(x, y);
    this.ctx.strokeStyle = this.getColor();
    this.ctx.lineWidth = 0;
    this.ctx.arc(x, y, radius, startAngle, endAngle);
    this.ctx.fillStyle = this.getColor();
    this.ctx.fill();
    this.ctx.globalCompositeOperation = "source-over";
  }

  getCoords(event) {
    let currentX: number, currentY: number;
    if (event.changedTouches) {
      // only for touch
      currentX =
        event.changedTouches[0].pageX -
        this.element.offsetLeft -
        this.element.clientLeft;
      currentY =
        event.changedTouches[0].pageY -
        this.element.offsetTop -
        this.element.clientTop;
    } else if (event.offsetX !== undefined) {
      currentX = event.offsetX;
      currentY = event.offsetY;
    } else {
      currentX = event.layerX - event.currentTarget.offsetLeft;
      currentY = event.layerY - event.currentTarget.offsetTop;
    }

    return [
      currentX * (this.element.width / this.element.clientWidth),
      currentY * (this.element.height / this.element.clientHeight),
    ];
  }
}

export interface GalleryOptionsMetadata {
  galleryAutoLoadMostRecent: boolean;
}

export interface DrawingPromptMetadata {
  drawingType: string;
  colorList: string[];
  premadeDrawing: string;
  canvasBackground: string;
  galleryOptions: GalleryOptionsMetadata;
}
