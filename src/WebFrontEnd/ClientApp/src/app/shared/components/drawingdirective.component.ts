import { Directive, ElementRef, HostListener, AfterViewInit, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import {throttle} from 'app/utils/throttle'
import PastColorsService from './colorpicker/pastColors';

const MaxUndoCount = 20;

@Directive({
    selector: '[appDrawing]',
})

export class DrawingDirective {
    ctx: any;
    @Input() lineColor: string;
    @Input() lineWidth: number;
    @Input() premadeDrawing: string;
    @Input() eraserMode: boolean;
    @Input() floodFillMode: boolean;
    @Input() galleryAutoLoadMostRecent: boolean;
    @Input() galleryEditor: boolean = false;
    @Output() drawingEmitter = new EventEmitter();
    defaultLineColor: string;
    element;
    private undoArray: string[] = [];
    private userIsDrawing:boolean;
    private lastX:number;
    private lastY: number;

    constructor(element: ElementRef) {
        console.log("Instantiating canvas");
        this.element = element.nativeElement;
        let pastColorsService = new PastColorsService();
        this.defaultLineColor = pastColorsService.getLastColor() || 'rgb(0,0,0)';
        this.ctx = element.nativeElement.getContext('2d');
        this.userIsDrawing = false;
    }

    loadImageString(imgStr){
      if (imgStr && this.ctx) {
          var img = new Image;
          var ctx = this.ctx;
          img.onload = function () {
              console.log("showing stored drawing");
              ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
              ctx.drawImage(img, 0, 0, ctx.canvas.width, ctx.canvas.height);
            }
          console.log("loading stored drawing");
          img.src = imgStr;     
          this.onImageChange(imgStr);
        }
    }
    ngOnInit() {
        this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
    }

    ngAfterViewInit(){
        if (this.galleryAutoLoadMostRecent) {
           /* will be handled by gallerytool */
        } 
        else if (this.premadeDrawing)
        {
          this.loadImageString(this.premadeDrawing)
        }
        else
        {
          this.onImageChange(null,false) /* so undo will work */
        }
    }

  emitImageChange(imgStr){
    // write the data to the emitter
    this.drawingEmitter.emit(imgStr);
  }

  onImageChange(imgStr:string, emitChange=true) {
    if (!imgStr){
        imgStr = this.element.toDataURL();
    }
    if (emitChange) {
        this.emitImageChange(imgStr);
    }

    // store it for an undo
    if (this.undoArray.length >= MaxUndoCount) { this.undoArray.shift(); }
    this.undoArray.push(imgStr);
    console.log('saved undo '+this.undoArray.length)
  }

  stopDrawing() {
    this.userIsDrawing = false;
    this.onImageChange(null);
  }

  handleClearUndo(){
    this.undoArray.length = 0;
    this.onImageChange(null,false) /* set base undo image */
  }

  onPerformUndo() {
    if (this.undoArray.length > 0) {
      if (this.undoArray.length>1) { this.undoArray.pop(); }  // the first call to undo would have the value we just stored, we want to go one back
      var img = new Image;
      var ctx = this.ctx;
      img.onload = function () {
          console.log("Drawing undo to canvas");
          ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
          ctx.drawImage(img, 0, 0, ctx.canvas.width, ctx.canvas.height);
        };
      console.log('Loading undo drawing '+this.undoArray.length)
      let imgStr = this.undoArray[this.undoArray.length-1];
      img.src = imgStr;
      this.emitImageChange(imgStr);
    }

  }
    @HostListener('mousedown', ['$event'])
    @HostListener('touchstart', ['$event'])
    onmousedown(event) {
//        event.preventDefault(); not needed since canvas does not care about mouse movements, and preventing affects gestures

      if (this.userIsDrawing) {
        console.log("down but already drawing");
        this.stopDrawing();
      }
      else {

        if (document.activeElement instanceof HTMLElement)  // pull focus from any input so it hides the keyboard on mobile
          document.activeElement.blur();

        [this.lastX, this.lastY] = this.getCoords(event);

        if (this.floodFillMode) {
            let clr = this.getColorAsRGB();
            var rgb = clr.split( ',' );
            let r=parseInt( rgb[0].substring(4) ) ; // skip rgb(
            let g=parseInt( rgb[1] ) ; // this is just g
            let b=parseInt( rgb[2] ) ; // parseInt scraps trailing )
            this.floodFill(Math.round(this.lastX),Math.round(this.lastY),r,g,b)
        } else {
            // begins new line
            //this.ctx.beginPath();

            this.drawCircle(this.lastX, this.lastY);

            this.userIsDrawing = true;
        }
      }
    }

    @HostListener('mousemove', ['$event'])
    @HostListener('touchmove', ['$event'])
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


  @HostListener('document:gesturestart')
  @HostListener('document:gestureend')
  ongesturestart() {
    console.log("gesture start");
    if (this.userIsDrawing) {
      this.stopDrawing();
      // we might want to call undo here? needs testing
    }
  }

  @HostListener('document:mouseup')
  @HostListener('document:touchend')
    @HostListener('mouseup')
    @HostListener('touchend')
    onmouseup() {
      if (this.userIsDrawing) {
//        event.preventDefault(); not needed since canvas does not care about mouse movements, and preventing affects gestures
        this.stopDrawing();
      }
    }

    @HostListener('mouseleave')
    @HostListener('touchleave')
    onmouseleave() {
    }
  
  @HostListener('window:scroll', ['$event']) 
  @throttle(200)
  onScrollEvent($event) {
    console.log("scrolling");
    if (this.userIsDrawing) {
      this.stopDrawing();
    }
  } 
    drawLine(lX, lY, cX, cY): void {
        if (this.eraserMode)
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

    getColorAsRGB(): string {
      let clr = this.getColor();

      if (clr.startsWith('rgb')) { return clr }

      if (clr.startsWith('hsl'))  {
        let sep = clr.indexOf(",") > -1 ? "," : " ";
        let hsl : any = clr.substr(4).split(")")[0].split(sep);
      
        let h : any = hsl[0];
        let s : any = hsl[1].substr(0,hsl[1].length - 1) / 100;
        let l : any = hsl[2].substr(0,hsl[2].length - 1) / 100;

        let c = (1 - Math.abs(2 * l - 1)) * s,
            x = c * (1 - Math.abs((h / 60) % 2 - 1)),
            m = l - c/2,
            r = 0,
            g = 0,
            b = 0;
    
        if (0 <= h && h < 60) {
          r = c; g = x; b = 0;
        } else if (60 <= h && h < 120) {
          r = x; g = c; b = 0;
        } else if (120 <= h && h < 180) {
          r = 0; g = c; b = x;
        } else if (180 <= h && h < 240) {
          r = 0; g = x; b = c;
        } else if (240 <= h && h < 300) {
          r = x; g = 0; b = c;
        } else if (300 <= h && h < 360) {
          r = c; g = 0; b = x;
        }
        r = Math.round((r + m) * 255);
        g = Math.round((g + m) * 255);
        b = Math.round((b + m) * 255);
      
        return "rgb(" + r + "," + g + "," + b + ")";
      }
    }


    drawCircle(x, y): void {
      if (this.eraserMode)
           this.ctx.globalCompositeOperation = "destination-out";
        this.ctx.beginPath();
        var radius = this.lineWidth/2.1; // Arc radius
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
        let currentX : number, currentY : number;
        if (event.changedTouches) { // only for touch
            currentX = event.changedTouches[0].pageX - this.element.offsetLeft - this.element.clientLeft;
            currentY = event.changedTouches[0].pageY - this.element.offsetTop - this.element.clientTop;

        }
        else if (event.offsetX !== undefined) {
            currentX = event.offsetX;
            currentY = event.offsetY;
        }
        else {
            currentX = event.layerX - event.currentTarget.offsetLeft;
            currentY = event.layerY - event.currentTarget.offsetTop;
        }

        return [currentX * (this.element.width / this.element.clientWidth), currentY * (this.element.height / this.element.clientHeight)];
    }

    colorData;

    floodFill(startX, startY, newR, newG, newB){
			let canvasWidth = this.ctx.canvas.width;
			let canvasHeight = this.ctx.canvas.height;
			this.colorData = this.ctx.getImageData(0, 0, canvasWidth, canvasHeight);

			var pixelIndex = (startY * canvasWidth + startX) * 4,
				r = this.colorData.data[pixelIndex],
				g = this.colorData.data[pixelIndex + 1],
				b = this.colorData.data[pixelIndex + 2],
				a = this.colorData.data[pixelIndex + 3];

			if (newR === r && newG === g && newB === b) {
				// Return because trying to fill with the same color
				return;
			}

			this.performFloodFill(startX, startY, r, g, b, a, newR, newG, newB);

      this.ctx.putImageData(this.colorData, 0, 0);
      this.onImageChange(null);
    }

    pixelsMatch(c1,c2): boolean {
      return (Math.abs(parseInt(c1)-parseInt(c2)) < 10);
    }

    matchTargetColor = function (pixelIndex, startR, startG, startB, startA, newR, newG, newB) {

			let r = this.colorData.data[pixelIndex];
			let g = this.colorData.data[pixelIndex + 1];
      let b = this.colorData.data[pixelIndex + 2];
      let a = this.colorData.data[pixelIndex + 3];

      if (startA < 225) { // we clicked on a transparent color, any other transparent is a match
        return (a < 225);  // note we may need to do color smoothing here if color does not exactly match clicked color
      } else {
			// If the current pixel matches the clicked color
  			if (this.pixelsMatch(r,startR) && this.pixelsMatch(g,startG) && this.pixelsMatch(b,startB)) {
	  			return true;
        }
      
        if (a < 255) {  // we are not a match and we clicked a solid color but we ran into a transparent, return false, but remove transparency
          this.setPixelColor(pixelIndex,r,g,b);
        }
      }


			return false;
		}

		setPixelColor(pixelIndex, r, g, b) {
			this.colorData.data[pixelIndex] = r;
			this.colorData.data[pixelIndex + 1] = g;
			this.colorData.data[pixelIndex + 2] = b;
			this.colorData.data[pixelIndex + 3] = 255;
		}



		performFloodFill(startX, startY, startR, startG, startB, startA, newR, newG, newB) {

			let newPos,
				x,
				y,
				pixelIndex,
        canvasWidth = this.ctx.canvas.width,
        canvasHeight = this.ctx.canvas.height,
        goLeft,
				goRight,
				pixelList = [[startX, startY]];


			while (pixelList.length) {

				newPos = pixelList.pop();
				x = newPos[0];
				y = newPos[1];

				// Get current pixel position
				pixelIndex = (y * canvasWidth + x) * 4;

				// Go up as long as the color matches and are inside the canvas
				while (y >= 0 && this.matchTargetColor(pixelIndex, startR, startG, startB, startA, newR, newG, newB)) {
					y -= 1;
					pixelIndex -= canvasWidth * 4;
				}

				pixelIndex += canvasWidth * 4;
				y += 1;
				goLeft = false;
				goRight = false;

				// Go down as long as the color matches and in inside the canvas
				while (y < canvasHeight && this.matchTargetColor(pixelIndex, startR, startG, startB, startA, newR, newG, newB)) {
					y += 1;

					this.setPixelColor(pixelIndex, newR, newG, newB);

					if (x > 0) {
						if (this.matchTargetColor(pixelIndex - 4, startR, startG, startB, startA, newR, newG, newB)) {
							if (!goLeft) {
								// Add pixel to stack
								pixelList.push([x - 1, y]);
								goLeft = true;
							}
						} else if (goLeft) {
							goLeft = false;
						}
					}

					if (x < (canvasWidth-1)) {
						if (this.matchTargetColor(pixelIndex + 4, startR, startG, startB, startA, newR, newG, newB)) {
							if (!goRight) {
								// Add pixel to stack
								pixelList.push([x + 1, y]);
								goRight = true;
							}
						} else if (goRight) {
							goRight = false;
						}
					}

					pixelIndex += canvasWidth * 4;
				}
			}
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
