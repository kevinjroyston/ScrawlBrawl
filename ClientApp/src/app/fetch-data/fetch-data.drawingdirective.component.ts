import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
    selector: '[appDrawing]'
})
export class DrawingDirective {
    constructor(element: ElementRef) {
        this.ctx = element.nativeElement.getContext('2d');
        this.drawing = false;
    }
    drawing:boolean;
    lastX:number;
    lastY: number;
    ctx: any;
    lineWidth: number = 10;

    @HostListener('mousedown', ['$event'])
    onmousedown(event) {
        if (event.offsetX !== undefined) {
            this.lastX = event.offsetX;
            this.lastY = event.offsetY;
        } else {
            this.lastX = event.layerX - event.currentTarget.offsetLeft;
            this.lastY = event.layerY - event.currentTarget.offsetTop;
        }

        // begins new line
        //this.ctx.beginPath();
        this.drawCircle(this.lastX, this.lastY);

        this.drawing = true;
    }

    @HostListener('mousemove', ['$event'])
    onmousemove(event) {
        if (this.drawing) {
            // get current mouse position
            var currentX;
            var currentY;
            if (event.offsetX !== undefined) {
                currentX = event.offsetX;
                currentY = event.offsetY;
            } else {
                currentX = event.layerX - event.currentTarget.offsetLeft;
                currentY = event.layerY - event.currentTarget.offsetTop;
            }
            this.drawLine(this.lastX, this.lastY, currentX, currentY);
            this.drawCircle(event.layerX, event.layerY);

            // set current coordinates to last one
            this.lastX = currentX;
            this.lastY = currentY;
        }
    }

    @HostListener('mouseup')
    onmouseup() {
        // stop drawing
        this.drawing = false;
    }

    drawLine(lX, lY, cX, cY): void {
        // line from
        this.ctx.beginPath();
        this.ctx.moveTo(lX, lY);
        // to
        this.ctx.lineTo(cX, cY);
        // color
        this.ctx.strokeStyle = "#4bf";
        //this.ctx.lineJoin = "round";
        this.ctx.lineWidth = this.lineWidth;
        // draw it
        this.ctx.stroke();
    }

    drawCircle(x, y): void {
        this.ctx.beginPath();
        var radius = this.lineWidth/2.1; // Arc radius
        var startAngle = 0; // Starting point on circle
        var endAngle = Math.PI * 2; // End point on circle

        this.ctx.moveTo(x,y);
        this.ctx.strokeStyle = "#4bf";
        this.ctx.lineWidth = 0;
        this.ctx.arc(x, y, radius, startAngle, endAngle);
        this.ctx.fillStyle = "#4bf";
        this.ctx.fill();
    }
}
