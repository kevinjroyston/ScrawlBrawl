import { Directive, ElementRef, HostListener, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

declare var angular: any;
@Directive({
    selector: '[appDrawing]',
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            multi: true,
            useExisting: forwardRef(() => DrawingDirective),
        }
    ]
})
export class DrawingDirective implements ControlValueAccessor{
    writeValue(obj: any): void {
        if (obj == null) {
            console.log("Clearing canvas");
            this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
            if (this.premadeDrawing) {
                var img = new Image;
                var ctx = this.ctx;
                img.onload = function () {
                    console.log("Drawing premade drawing to canvas");
                    ctx.drawImage(img, 0, 0, ctx.canvas.width, ctx.canvas.height);
                };
                console.log("Loading premade drawing");
                img.src = this.premadeDrawing;
            }
        } else {

        }
    }
    registerOnChange(fn: any): void {
        this.onChange = fn; 
    }
    registerOnTouched(fn: any): void {

    }
    setDisabledState?(isDisabled: boolean): void {

    }
    constructor(element: ElementRef) {
        console.log("Instantiating canvas");
        this.element = element.nativeElement;
        this.ctx = element.nativeElement.getContext('2d');
        this.drawing = false;
    }
    drawing:boolean;
    lastX:number;
    lastY: number;
    ctx: any;
    lineWidth: number = 10;
    @Input() lineColor: string;
    @Input() premadeDrawing: string;
    defaultLineColor: string = "rgba(87,0,132,255)";
    onChange;
    element;

    @HostListener('mousedown', ['$event'])
    @HostListener('touchstart', ['$event'])
    onmousedown(event) {
        console.log("down/start");
        event.preventDefault();
        if (event.changedTouches) {                      // only for touch
            this.lastX = event.changedTouches[0].pageX - this.element.offsetLeft - this.element.clientLeft;
            this.lastY = event.changedTouches[0].pageY - this.element.offsetTop - this.element.clientTop;
        } else {
            if (event.offsetX !== undefined) {
                this.lastX = event.offsetX;
                this.lastY = event.offsetY;
            } else {
                this.lastX = event.layerX - event.currentTarget.offsetLeft;
                this.lastY = event.layerY - event.currentTarget.offsetTop;
            }
        }

        // begins new line
        //this.ctx.beginPath();
        this.drawCircle(this.lastX, this.lastY);

        this.drawing = true;
    }

    @HostListener('mousemove', ['$event'])
    @HostListener('touchmove', ['$event'])
    onmousemove(event) {
        //console.log("move");
        event.preventDefault();
        if (this.drawing) {
            // get current mouse position
            var currentX;
            var currentY;
            if (event.changedTouches) {                      // only for touch
                currentX = event.changedTouches[0].pageX - this.element.offsetLeft - this.element.clientLeft;
                currentY = event.changedTouches[0].pageY - this.element.offsetTop - this.element.clientTop;
            }else if (event.offsetX !== undefined) {
                currentX = event.offsetX;
                currentY = event.offsetY;
            } else {
                currentX = event.layerX - event.currentTarget.offsetLeft;
                currentY = event.layerY - event.currentTarget.offsetTop;
            }
            this.drawLine(this.lastX, this.lastY, currentX, currentY);
            this.drawCircle(currentX, currentY);

            // set current coordinates to last one
            this.lastX = currentX;
            this.lastY = currentY;
        }
    }

    @HostListener('mouseup')
    @HostListener('touchend')
    onmouseup() {
        console.log("up/end");
        event.preventDefault();
        // stop drawing
        this.drawing = false;
        this.onChange(this.element.toDataURL());
    }

    @HostListener('mouseleave')
    onmouseleave() {
        console.log("mouseleave");
        event.preventDefault();
        // stop drawing
        this.drawing = false;
        this.onChange(this.element.toDataURL());
    }

    drawLine(lX, lY, cX, cY): void {
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
    }

    getColor(): string {
        if (this.lineColor && this.lineColor != "") {
            return this.lineColor;
        }
        return this.defaultLineColor;
    }

    drawCircle(x, y): void {
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
    }
}
