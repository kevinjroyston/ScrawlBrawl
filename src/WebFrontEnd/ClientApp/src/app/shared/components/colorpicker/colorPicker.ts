class ColorPickerConstants {
    static RGB_MIN : number = 0;
    static RGB_MAX : number = 255;
    static HEX_MIN : number = 0;
    static HEX_MAX : number = 320; // This is a circle, so max should be lowered based on num colors per row  (360/(numColorsPerRow+1)) I lowered it extra to make yellows look better
    static LUMINOSITY_MIN : number = 14;
    static LUMINOSITY_MAX : number = 80;
    static MAX_SATURATION : number = 100;
    static MIN_SATURATION : number = 65;
    static numColorsPerRow : number = 12;
    static numShadeOfColor : number = 10;
}

export default class ColorPickerService {

    generateGrayScale = (numSquares: number = ColorPickerConstants.numColorsPerRow) : string[] => {
        let grayScale = []
    
        for (let square = 0; square <= numSquares - 2; square += 1) {
            let squareValue = square * (ColorPickerConstants.RGB_MAX - ColorPickerConstants.RGB_MIN)/(numSquares - 2);
            grayScale.push(`rgb(${squareValue}, ${squareValue}, ${squareValue})`)
        }
    
        return grayScale
    }
    
    generateRainbow = (numColors : number = ColorPickerConstants.numColorsPerRow, numShades: number = ColorPickerConstants.numShadeOfColor) : string[][] => {

        let rainbowColors = []
        let baseColors = []

        for (
            let i = 0; 
            i < numColors; 
            i += 1
        ) {
            baseColors.push(ColorPickerConstants.HEX_MIN + i*((ColorPickerConstants.HEX_MAX-ColorPickerConstants.HEX_MIN)*1.0/(numColors-1)));
        }

        for (let color of baseColors) {
            let colorRow = []
            for (
                let i = 0; 
                i < numShades; 
                i += 1
            ) {
                let luminosity = ColorPickerConstants.LUMINOSITY_MIN + i *((ColorPickerConstants.LUMINOSITY_MAX-ColorPickerConstants.LUMINOSITY_MIN)*1.0/(numShades-1));
                let saturation = ColorPickerConstants.MIN_SATURATION + (numShades - 1 - i )*((ColorPickerConstants.MAX_SATURATION - ColorPickerConstants.MIN_SATURATION)*1.0/(numShades-1));
                colorRow.push(`hsl(${color}, ${saturation}%, ${luminosity}%)`)
            }
            rainbowColors.push(colorRow);
        }

        return rainbowColors
    }
}
