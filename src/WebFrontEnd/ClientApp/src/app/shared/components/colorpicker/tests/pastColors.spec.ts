import PastColorsService from "../pastColors";

describe('pastColors', () => {
    let pastColorsService;
    let colors = {
        black: 'rgb(0,0,0)',
        black1: 'rgb(0,0,1)',
        black2: 'rgb(0,0,2)',
        black3: 'rgb(0,0,3)',
        black4: 'rgb(0,0,4)',
        black5: 'rgb(0,0,5)',
        black6: 'rgb(0,0,6)'
    }

    beforeEach(async ()=> {
        pastColorsService = new PastColorsService();
        localStorage.removeItem('colorPicker')
    })

    describe('addColor', async() => {
        it ('should add color to localstorage', async() => {
            let color = colors.black;
            pastColorsService.addColor(color);
            expect(localStorage.getItem('colorPicker')).toEqual(JSON.stringify([color]))
        })
    })
    
    describe('retrievePastColors', async() => {
        it ('should retrieve past colors used', async () => {
            let colorPicker = `["${colors.black1}"]`;
            localStorage.setItem('colorPicker', colorPicker);
            let pastColors = pastColorsService.retrievePastColors();
            expect(pastColors).toEqual([colors.black1]);
        })

        it ('should retrieve empty array if not parseable', async() => {
            let colorPicker = `[${colors.black1}]`;
            localStorage.setItem('colorPicker', colorPicker);
            let pastColors = pastColorsService.retrievePastColors();
            expect(pastColors).toEqual([]);
        })
    })

    describe('getLastColor', async() => {
        it('should get last color used', async() => {
            let color = colors.black;
            let colorArray = [color];
            localStorage.setItem('colorPicker', JSON.stringify(colorArray));
            let lastColor = pastColorsService.getLastColor();
            expect(lastColor).toEqual(color);
        })
    })

    describe('handleIfExists', async () => {
        it ('should check if past color used', async () => {
            let colorArray = [colors.black, colors.black1, colors.black2, colors.black3, colors.black4, colors.black5]
            localStorage.setItem('colorPicker', JSON.stringify(colorArray));
            let pickedColors = pastColorsService.handleIfExists(colors.black3);
            expect(pickedColors).toEqual([colors.black3, colors.black, colors.black1, colors.black2, colors.black4, colors.black5])
        })

        it ('should remove color at end if capacity filled', async () => {
            let colorArray = [colors.black, colors.black1, colors.black2, colors.black3, colors.black4, colors.black5]
            localStorage.setItem('colorPicker', JSON.stringify(colorArray));
            let pickedColors = pastColorsService.handleIfExists(colors.black6);
            expect(pickedColors).toEqual([colors.black6, colors.black, colors.black1, colors.black2, colors.black3, colors.black4])
        })
    })

    describe('deleteColors', async () => {
        it ('should delete past colors', async () => {
            let colorPicker = 'colorPicker'
            localStorage.setItem('colorPicker', colorPicker)
            pastColorsService.deleteColors()
            expect(localStorage.getItem('colorPicker')).toEqual(null);
        })
    })
})