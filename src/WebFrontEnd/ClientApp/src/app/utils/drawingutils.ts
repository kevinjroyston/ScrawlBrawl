export function convertColorToRGB(clr:string): string {
/* expects a string in the format #rgb or rgb(r,g,b) or   hsl(h,s,l) only supports the limited hsl format we currently use */
    if (clr.startsWith('rgb')) { return clr }
    if (clr.startsWith('#'))  {
        var r,g,b;
        if ( clr.charAt(0) == '#' ) {
          clr = clr.substr(1);
        }

        r = clr.charAt(0) + clr.charAt(1);
        g = clr.charAt(2) + clr.charAt(3);
        b = clr.charAt(4) + clr.charAt(5);

        r = parseInt( r,16 );
        g = parseInt( g,16 );
        b = parseInt( b,16);
        return "rgb(" + r + "," + g + "," + b + ")";
    }

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
    return "rgb(0,0,0)";  /* only here if type is not hsl or rgb */
}

export function convertColorToHexRGB(clr:string): string {
  if (clr.startsWith('#')) { return clr }  // we already are in hex, done
  clr = convertColorToRGB(clr);  // convert possible hsl to rgb()
  const rgb2hex = (rgb) => `#${rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/).slice(1).map(n => parseInt(n, 10).toString(16).padStart(2, '0')).join('')}`
  return rgb2hex(clr);
}

var colorData = null;

export function floodFill(ctx, startX, startY, newR, newG, newB):boolean{
    let canvasWidth = ctx.canvas.width;
    let canvasHeight = ctx.canvas.height;
    if (colorData) { return false } /* already filling */
    colorData = ctx.getImageData(0, 0, canvasWidth, canvasHeight);

    var pixelIndex = (startY * canvasWidth + startX) * 4,
        r = colorData.data[pixelIndex],
        g = colorData.data[pixelIndex + 1],
        b = colorData.data[pixelIndex + 2],
        a = colorData.data[pixelIndex + 3];

    if (!(newR === r && newG === g && newB === b && a==255)) { // they did not click on the same color
        performFloodFill(ctx, startX, startY, r, g, b, a, newR, newG, newB);
        ctx.putImageData(colorData, 0, 0);
    }
    colorData = null;
    return true;
}

function pixelsMatch(c1,c2): boolean {
  return (Math.abs(parseInt(c1)-parseInt(c2)) < 25);
}
function transparenciesMatch(t1,t2): boolean {
  return (Math.abs(parseInt(t1)-parseInt(t2)) < 128);
}

function matchTargetColor(pixelIndex, startR, startG, startB, startA, newR, newG, newB) {

  let r = colorData.data[pixelIndex];
  let g = colorData.data[pixelIndex + 1];
  let b = colorData.data[pixelIndex + 2];
  let a = colorData.data[pixelIndex + 3];

  if (startA < 225) { // we clicked on a transparent color, any other transparent is a match
    return (a < 225);  // note we may need to do color smoothing here if color does not exactly match clicked color
  } else { 
    if (pixelsMatch(r,startR) && pixelsMatch(g,startG) && pixelsMatch(b,startB) && transparenciesMatch(a,startA)) { // current pixel matches the clicked color
       return true;
    }
  
    if (a < 255) {  // we are not a match and we clicked a solid color but we ran into a transparent, return false, but remove transparency
//      colorData.data[pixelIndex + 3] = 255;
//      setPixelColor(pixelIndex,startR,startG,startB);
    }
  }

  return false;
}

function setPixelColor(pixelIndex, r, g, b) {
    colorData.data[pixelIndex] = r;
    colorData.data[pixelIndex + 1] = g;
    colorData.data[pixelIndex + 2] = b;
    colorData.data[pixelIndex + 3] = 255;
}



function performFloodFill(ctx,startX, startY, startR, startG, startB, startA, newR, newG, newB) {

let checkPixel,
    x,
    y,
    pixelIndex,
    canvasWidth = ctx.canvas.width,
    canvasHeight = ctx.canvas.height,
    goLeft,
    goRight,
    pixelList = [[startX, startY]],
    sanityCheck = canvasWidth * canvasHeight;

    while (pixelList.length && pixelList.length <= sanityCheck) {
        checkPixel = pixelList.pop();
        x = checkPixel[0];
        y = checkPixel[1];

        // Get current pixel position
        pixelIndex = (y * canvasWidth + x) * 4;

        // Go up as long as the color matches and are inside the canvas
        while (y >= 0 && matchTargetColor(pixelIndex, startR, startG, startB, startA, newR, newG, newB)) {
            y -= 1;
            pixelIndex -= canvasWidth * 4;
        }

        pixelIndex += canvasWidth * 4;
        y += 1;
        goLeft = false;
        goRight = false;

        // Go down as long as the color matches and in inside the canvas
        while (y < canvasHeight && matchTargetColor(pixelIndex, startR, startG, startB, startA, newR, newG, newB)) {
            y += 1;

            setPixelColor(pixelIndex, newR, newG, newB);

            if (x > 0) {
                if (matchTargetColor(pixelIndex - 4, startR, startG, startB, startA, newR, newG, newB)) {
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
                if (matchTargetColor(pixelIndex + 4, startR, startG, startB, startA, newR, newG, newB)) {
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


export function isColorCloseToClassBackground(colorString: string, classIdentifier: string, threshold: number = 10): boolean {
  const cssColor = getBackgroundColorValue(classIdentifier);
  const rgb1 = parseColor(colorString);
  const rgb2 = parseColor(cssColor);
  
  if (!rgb1 || !rgb2) return false;

  const distance = Math.sqrt(
    Math.pow(rgb1.r - rgb2.r, 2) +
    Math.pow(rgb1.g - rgb2.g, 2) +
    Math.pow(rgb1.b - rgb2.b, 2)
  );

  return distance <= threshold;
}

function getBackgroundColorValue(className: string): string {
  if (typeof window === 'undefined' || !window.getComputedStyle) {
    console.warn('getComputedStyle is not available');
    return '';
  }

  const element = document.querySelector(`.${className}`);

  if (!element) {
    console.warn(`No element found with class ${className}`);
    return '';
  }

  const computedStyle = getComputedStyle(element);

  const backgroundColor = computedStyle.backgroundColor;

  if (!backgroundColor) {
    console.warn(`No background-color found for element with class ${className}`);
    return '';
  }

  return backgroundColor;
}


function parseColor(color: string): { r: number, g: number, b: number } | null {
  if (color.startsWith('#')) {
    return hexToRgb(color);
  }
  if (color.startsWith('rgb')) {
    return parseRgbString(color);
  }
  return null;
}

function hexToRgb(hex: string): { r: number, g: number, b: number } | null {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result ? {
    r: parseInt(result[1], 16),
    g: parseInt(result[2], 16),
    b: parseInt(result[3], 16)
  } : null;
}

function parseRgbString(colorString: string): { r: number, g: number, b: number } | null {
  const match = colorString.match(/rgb\((\d+\.?\d*),\s*(\d+\.?\d*),\s*(\d+\.?\d*)\)/);
  if (!match) return null;

  return {
    r: Math.round(parseFloat(match[1])),
    g: Math.round(parseFloat(match[2])),
    b: Math.round(parseFloat(match[3]))
  };
}