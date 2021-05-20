

  export function setCSSDisplayForSelector(findSelector,setDisplay):void{
    {
      for( var i in document.styleSheets ){
        var sheet = document.styleSheets[i];
        var rules;
        if (sheet) {
        try {
            rules = sheet.cssRules || sheet.rules;
  //        rules = sheet.cssRules ? sheet.cssRules : sheet.rules;
          
        } catch (error) {
          rules = null;
        }
        if (rules) {
        for (var r=0; r < rules.length; r++){
            if (((<CSSStyleRule>rules[r]).selectorText) && ((<CSSStyleRule>rules[r]).selectorText.toLowerCase()==findSelector.toLowerCase())) {
              (<CSSStyleRule>rules[r]).style.display = setDisplay;
              break;
            }
          }
        }
        }
      }    
    }
  }
