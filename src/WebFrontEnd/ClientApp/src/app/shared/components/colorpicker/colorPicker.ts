class ColorPickerConstants {
    static RGB_MIN : number = 0;
    static RGB_MAX : number = 255;
    static HEX_MIN : number = 0;
    static HEX_MAX : number = 360;
    static LUMINOSITY_MIN : number = 10;
    static LUMINOSITY_MAX : number = 90;
    static SATURATION : number = 100;
    static numColorsPerRow : number = 11;
    static numShadeOfColor : number = 9;
}

export default class ColorPickerService {

    generateGrayScale = (numSquares: number = ColorPickerConstants.numColorsPerRow) : string[] => {
        let grayScale = []
    
        for (let value = ColorPickerConstants.RGB_MIN; value <= ColorPickerConstants.RGB_MAX - ColorPickerConstants.RGB_MAX/numSquares; value += ColorPickerConstants.RGB_MAX/numSquares) {
          grayScale.push(`rgb(${value}, ${value}, ${value})`)
        }
    
        return grayScale
    }
    
    generateRainbow = (numColors : number = ColorPickerConstants.numColorsPerRow, numSquares: number = ColorPickerConstants.numShadeOfColor) : string[][] => {

        let rainbowColors = []
        let baseColors = []

        for (let hex = ColorPickerConstants.HEX_MIN; hex <= ColorPickerConstants.HEX_MAX - ColorPickerConstants.HEX_MAX/numColors; hex += ColorPickerConstants.HEX_MAX/numColors) {
            baseColors.push(hex);
        }

        for (let color of baseColors) {
            let colorRow = []
            for (let luminosity = ColorPickerConstants.LUMINOSITY_MIN; luminosity <= ColorPickerConstants.LUMINOSITY_MAX; luminosity += ColorPickerConstants.LUMINOSITY_MAX/numSquares) {
            colorRow.push(`hsl(${color}, ${ColorPickerConstants.SATURATION}%, ${luminosity}%)`)
            }
            rainbowColors.push(colorRow);
        }

        return rainbowColors
    }
}
