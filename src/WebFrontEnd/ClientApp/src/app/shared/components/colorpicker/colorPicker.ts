export default class ColorPickerService {

    generateGrayScale = () : string[][] => {
        let grayScale = []
    
        let num_shades=8;
        for (let square = num_shades-1; square >=0; square -= 1) {
            let squareValue = 255.0/(num_shades-1) * square;
            grayScale.push(`rgb(${squareValue}, ${squareValue}, ${squareValue})`)
        }
        let bothGrayScales = [];
        bothGrayScales.push(grayScale);
        bothGrayScales.push(gray_scale2);
        return bothGrayScales;
    }
    
    generateRainbow = () : string[][] => {
        return ColorPickerConstants.COLORS
    }

    public colorIsInPallette(clr) {
        for (var i=0; i < ColorPickerConstants.COLORS.length; i++){
          if (ColorPickerConstants.COLORS[i].includes(clr)) return true;
        }
        let bothGrayScales = this.generateGrayScale();
        for (var i=0; i < bothGrayScales.length; i++){
            if (bothGrayScales[i].includes(clr)) return true;
          }
  
        return false;
      }


     
    
}



// Below are just colors for the color picker

/*  Flat color palette (full) downloaded from
 *  http://htmlcolorcodes.com/color-charts/
 *
 *  Check out our powerful color picker
 *  http://htmlcolorcodes.com/color-picker/
 *
 *  Want to learn more about colors in SCSS?
 *  http://htmlcolorcodes.com/tutorials/
 *
 *  - - - - - - - - - - - - - - - - - - 
 *  Credits:
 *
 *  Original color palette by
 *  http://designmodo.github.io/Flat-UI/
 *
 *  - - - - - - - - - - - - - - - - - -
 *  How to use:
 *  
 *  Include this file with your other .scss files:
 *
 *  @import "flat_colors_full";
 *  
 *  and use the color variables in your stylesheets:
 *	
 *  .class {
 *    color: $turquoise-500;
 *  }
 *
 *  - - - - - - - - - - - - - - - - - -
 *  Note:
 *
 *  $color and $color-500 are the same, simply there
 *  for your convenience.
 */

 let gray_scale2 =["#d6dbdf","#aeb6bf","#85929e","#5d6d7e","#34495e","#2e4053","#2A3747","#212f3c"];

 let turquoise =["#d1f2eb","#a3e4d7","#76d7c4","#48c9b0","#1abc9c","#17a589","#148f77","#117864","#0e6251"];
 //let green_sea =["#d0ece7","#a2d9ce","#73c6b6","#45b39d","#16a085","#138d75","#117a65","#0e6655","#0b5345"];
 let emerald =["#d5f5e3","#abebc6","#82e0aa","#58d68d","#2ecc71","#28b463","#239b56","#1d8348","#186a3b"];
 let nephritis =["#d4efdf","#a9dfbf","#7dcea0","#52be80","#27ae60","#229954","#1e8449","#196f3d","#145a32"];
 let peter_river =["#d6eaf8","#aed6f1","#85c1e9","#5dade2","#3498db","#2e86c1","#2874a6","#21618c","#1b4f72"];
 let belize_hole =["#d4e6f1","#a9cce3","#7fb3d5","#5499c7","#2980b9","#2471a3","#1f618d","#1a5276","#154360"];
 //let amethyst =["#ebdef0","#d7bde2","#c39bd3","#af7ac5","#9b59b6","#884ea0","#76448a","#633974","#512e5f"];
 let wisteria =["#e8daef","#d2b4de","#bb8fce","#a569bd","#8e44ad","#7d3c98","#6c3483","#5b2c6f","#4a235a"];
 let midnight_blue =["#d5d8dc","#abb2b9","#808b96","#566573","#2c3e50","#273746","#212f3d","#1c2833","#17202a"];
 let sunflower =["#fcf3cf","#f9e79f","#f7dc6f","#f4d03f","#f1c40f","#d4ac0d","#b7950b","#9a7d0a","#7d6608"];
 let orange =["#fdebd0","#fad7a0","#f8c471","#f5b041","#f39c12","#d68910","#b9770e","#9c640c","#7e5109"];
 let carrot =["#fae5d3","#f5cba7","#f0b27a","#eb984e","#e67e22","#ca6f1e","#af601a","#935116","#784212"];
 let pumpkin =["#f6ddcc","#edbb99","#e59866","#dc7633","#d35400","#ba4a00","#a04000","#873600","#6e2c00"];
 let alizarin =["#fadbd8","#f5b7b1","#f1948a","#ec7063","#e74c3c","#cb4335","#b03a2e","#943126","#78281f"];
 let pomegranate =["#f2d7d5","#e6b0aa","#d98880","#cd6155","#c0392b","#a93226","#922b21","#7b241c","#641e16"];
// let clouds =["#fbfcfc","#f7f9f9","#f4f6f7","#f0f3f4","#ecf0f1","#d0d3d4","#b3b6b7","#979a9a","#7b7d7d"];
 //let silver =["#f2f3f4","#e5e7e9","#d7dbdd","#cacfd2","#bdc3c7","#a6acaf","#909497","#797d7f","#626567"];
 let concrete =["#eaeded","#d5dbdb","#bfc9ca","#aab7b8","#95a5a6","#839192","#717d7e","#5f6a6a","#4d5656"];
 let asbestos =["#e5e8e8","#ccd1d1","#b2babb","#99a3a4","#7f8c8d","#707b7c","#616a6b","#515a5a","#424949"];
 
 /*  Want more? Check our our other color palettes at
  *  http://htmlcolorcodes.com/color-charts/
  */

 class ColorPickerConstants {
    static COLORS : string[][] = [pomegranate,alizarin,pumpkin,carrot,orange,sunflower,emerald,nephritis,turquoise,peter_river,belize_hole,wisteria];
}