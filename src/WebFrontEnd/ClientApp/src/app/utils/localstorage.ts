

  export function fetchLocalStorage(key,id):string{
    return localStorage.getItem(key+'-'+id);
  }

  export function fetchURLParam(paramName):string{
      const urlParams = new URLSearchParams(window.location.search);
      return urlParams.get(paramName);
  }

  export function storeLocalStorage(key,id,value):void{
      localStorage.setItem(key+'-'+id,value);
  }

