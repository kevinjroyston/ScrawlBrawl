class PastColorsConstants {
    static MAX_COLORS : number = 6;
}

export default class PastColorsService {

    getLastColor = () : string | null => {
        let pastColors = this.retrievePastColors()
        return pastColors.length === 0 ? null : pastColors[0]
    }
    
    retrievePastColors = () => {
        if (localStorage.getItem('colorPicker')) {
            try {
              return JSON.parse(localStorage.getItem('colorPicker'))
            } 
            catch(e) {}
        } 
        
        return []
    }

    addColor = (color: string) : string[] => {
        let pickedColors : string[];
    
        if (localStorage.getItem('colorPicker') === null) {
          pickedColors = [color]
        } else {
          pickedColors = this.handleIfExists(color)
        }

        localStorage.setItem('colorPicker', JSON.stringify(pickedColors))
        return pickedColors;
    }

    handleIfExists = (color: string) : string[] => {
        let pickedColors = JSON.parse(localStorage.getItem('colorPicker'));
        let index = pickedColors.indexOf(color);

        if (index > -1) {
            pickedColors.splice(index, 1);
            pickedColors.unshift(color)
            return pickedColors;
        }

        if (pickedColors.length >= PastColorsConstants.MAX_COLORS) {
            pickedColors.pop();
        }

        return [color].concat(pickedColors);
    }

    deleteColors = () : void => {
        localStorage.removeItem('colorPicker')
    }
}