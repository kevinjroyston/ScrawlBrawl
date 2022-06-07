namespace Galleries  {

    export const favorites: string = 'Favorites';
    export const recent: string = 'Recent';
    export const samples: string = 'Samples';

    export enum GalleryPanelType {
        SAMPLES = "Samples",
        RECENT = "Recent",
        FAVORITES = "Favorite"
    }

    export interface GalleryDrawing {
        image: string;
    }

    export class GalleryType {
        drawingType: string;
        galleryDesc: string;
        imageWidth: number;
        imageHeight: number;
        maxLocalFavorites: number;
        maxLocalRecent: number;
        canvasBackground: string;
    }

    export const galleryTypes:ReadonlyArray<GalleryType> = 
     [
        {
            drawingType:"PROFILE",
            galleryDesc: "Profile Pictures",
            imageWidth: 300,
            imageHeight: 300,
            maxLocalFavorites: 6,
            maxLocalRecent: 4,    
            canvasBackground: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASwAAAEsCAYAAAB5fY51AAAABmJLR0QA/wAiACLdjq+ZAAAACXBIWXMAAC4jAAAuIwF4pT92AAADAklEQVR42u3WMQ3AQBAEsU/4cx0IFxJXvBQbwhajPQeWVVONJdj2mgAQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAMECECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAMECECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsADBAhAsAMECfuipxgyAhwUAN6vGc8fDAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAAQLECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAAQLECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsADBMgEgWACCBQgWgGAB7PgAlGYVUG63OlQAAAAASUVORK5CYII=",
        },

        {
            drawingType:"HEAD",
            galleryDesc: "Heads",
            imageWidth: 360,
            imageHeight: 240,
            maxLocalFavorites: 6,
            maxLocalRecent: 4,    
            canvasBackground: "data:image/png;base64,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",
//          tick marks  canvasBackground: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPAAAAB4CAYAAADMtn8nAAACHUlEQVR4nO3ZwWoTYRSA0ZumIkU0BTe+/4O5FMFNI0IraTpltItmFzfGT85Zziz+y9x8DEk2y7J8npnbAWru1oAfZuat1UHOzzXg48xc2R3kPK3h7u0NkvbevBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ9i15f19x8eZx/uZ4+H06O2bmeubme0/upXq3P8zj/wC1gi+f52538/M8nL+ZuZmN/Ph08z2vbk5j4AvYH2DPexnfnw7DWEzM+8+mpvzCfgC1s/+sswsT6ch/Lpmbv6AgC9gs5m52v7+7vg6hPXaes/cnMuv0BAmYAgTMIQJGMIEDGFrwDsLhKTdGvDB7iDpsP4P/GVmbu0PYmbungEVxFF+WFuqEgAAAABJRU5ErkJggg==",
        },
        {
            drawingType:"BODY",
            galleryDesc: "Bodies",
            imageWidth: 360,
            imageHeight: 240,
            maxLocalFavorites: 6,
            maxLocalRecent: 4,
            canvasBackground: "data:image/png;base64,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"
                                +"h6k2RkY/jyNThWVp/DqU/y0q9Xl/lVjwKABAp0IajRCrQ/AfTl3+Nnn6m3o4pLKbT6WsqYmiW/kgNVaLizgmlUunzWq22AqEGwD7QBmciFej33nuPlypLbuOpVOrCd99995GKiO3tbf6/ym7jU1NTldnZ2T8UAMAq0AZnIhVoHUNYMx3DJ4JmUqtLGp6t+QS8evXqgum4g4OD2uLiorjC4QCMG9AGZyL3QdMSgWN7vbJuyidOnPjMxhKCfVy8+eB1AohdWlYhDhmAiIA2vE3kAs3B3eTDWaM/sONxaJF9PbSsCa15pH4v9nEZTy5/NrpY1lC8H4DogDY4/H8qBubm5vYODw+/9nHo7qVLl0Lr/M3ppMpH1amTJ0+uwbUBQPRAG44SWxx0tVrlL2Td4zDHZQTtoH5Ay5EyL0nYZ9T/vQ5sL/IYHzP8Op71Wq3WbWWg0+msnz179rkCAMQCtOENKRUz/EXSsmGVZrDhAt07dKLemkk5gH0gNtEIV8VymmVpk+ET8mMdWR7x0oVnx6hPQFyFagDoI/UanHRt6P3fKmZ4tnznnXe+UkPLC/LzrDsdT1+sb79TJpNxPJZOwDdDv9qlXdmvYDkDIAdog5BiSex3oi+dT0QvzIa+6EcGP4/vvHjajXU8Vi+hev8Xh8vw/w2fMwDymHRtsF5u1C96V/TO06dPa/TjeIyOfxxl53aeX+O048ohPbR0aVy+fDm0jQYAQPhMsjaIEeg+PGO6jeni2CNBXzRnKL31ResTI0GcEWsNYqXZbN5QCWACtUGeQHswctxjKpUqKyFfthPomgLi5sMPPxyHVm5jpw1MogS6VCrVfv31V3baF2kZ8i7NmtxldziwnC+2LvmYmoVC4RkdCxcGAGMOtAEAAAAAAAAAAAAAAAAAAAAAMBbEXosDABAvjUbDNUStXC4j0iFGkhYHPXagWBKIm1ar9YlhGNdgjIioxQEAAOBtINAAACAUCDQAAAgFAg0AAEKBQAMAgFAg0AAAIBQINAAACAUCDQAAQoFAAwCAUCDQAAAgFAg0AAAIBQINAABCgUADAIBQINAAACCU/wdm3ejmSOcyHQAAAABJRU5ErkJggg==",
//          tick marks  canvasBackground: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPAAAADwCAYAAAA+VemSAAAEKUlEQVR4nO3a22ocRxRA0dMamUQYWwl+8f9/XDAYksgxyEGXDi0PiSYgUF4y2pq1HksPOkzV1qWrl3Vdf52Zn4f/zZ9/zHz5NHN9NTPr/rsuMxeXM+8/zvzw7mXuRXXuV+y381P/BI7h7mbm29XM18+HISwz8/aDuXk+AR/BdvbXdWa9PwzhYc3c/AcCPoJlmTnbzezeHIawrW1fMzfPdeaTgi4BQ5iAIUzAECZgCPMU+gi2a5f7u+/3qo+f5m5r6wu+j6nO/ZoJ+AiW/ZXMcvavFyL2L0WYm+cS8BFs96g/Xu4beBTCtvZwx2punsm70Edwdztze73/U/SRLYLzi5ndC/2xWp37FfMu9DFsB30XfPG/Ovdr5ik0hAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ9j5KW/e3e3M7fXM3c3h+u7NzPnFzO6kP52Xwz497aSP6HYovnyaub6amXW/uMxcXM68/zize3fkAXlgn5522r+Bb2a+Xc18/Xx4MJaZefvhyMPxN/v0tJMOeDsL6zqz3h8ejIe1I8/GP+zT00464GWZOdt9/1/q8cHY1rav8TLYp6d5Cg1hAoYwAUOYgCFMwBC2BXxpAyHpcgv4xt5B0s12D/zLzPxk/yBm5ve/AINppA0rSIxSAAAAAElFTkSuQmCC",
        },

        {
            drawingType:"LEGS",
            galleryDesc: "Legs",
            imageWidth: 360,
            imageHeight: 240,
            maxLocalFavorites: 6,
            maxLocalRecent: 4,    
            canvasBackground: "data:image/png;base64,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",
//          tick marks  canvasBackground: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPAAAADwCAYAAAA+VemSAAADhElEQVR4nO3aTYvTUBSA4ZNORUT8ws38/x8ngqCOCiqljVQGYRZdqn0nz7PM6ibnvuTSdFnX9Tgzu9mgn19nvryf+X43M+v9/S8zz17NvLydefpii0/l+pjTRaf9VuM9Ox5mftzNfPvwcGMsM/P87X9eHH+Y00W7/ZUu7J8474V1nVlPDzfG72uP/N5LzOmy/f0jWa51gX/TsszsbmZunjzcGOdryyafyHUyp4vWzR6f4TEQMIQJGMIEDGEChrDdVn+Bhkdg8QaGsPN34NNWj9K+LzaY00Wn83+hP87MmytdIHDZJ0doCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQtqzrehQyJJ3O4R7MDpIO+5l5NzOvzQ9iZj7/AsBRSqqzpLyAAAAAAElFTkSuQmCC",

        },

        {
            drawingType:"GENERIC",
            galleryDesc: "Images (Favorites of this type not currently used)",
            imageWidth: 300,
            imageHeight: 300,
            maxLocalFavorites: 4,
            maxLocalRecent: 2,    
            canvasBackground: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASwAAAEsCAYAAAB5fY51AAAABmJLR0QA/wAiACLdjq+ZAAAACXBIWXMAAC4jAAAuIwF4pT92AAADAklEQVR42u3WMQ3AQBAEsU/4cx0IFxJXvBQbwhajPQeWVVONJdj2mgAQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAMECECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAMECECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsADBAhAsAMECfuipxgyAhwUAN6vGc8fDAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAAQLECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsAAECxAsAMECECxAsAAEC0CwAMECECwAwQIEC0CwAAQLECwAwQIEC0CwAAQLECwAwQIQLECwAAQLQLAAwQIQLADBAgQLQLAABAsQLADBAhAsQLAABAtAsADBAhAsAMECBAtAsADBMgEgWACCBQgWgGAB7PgAlGYVUG63OlQAAAAASUVORK5CYII=",
        },
    ];
    export function galleryFromDrawingType(drawingType):GalleryType{
        var returnGallery = galleryTypes[0];
        galleryTypes.forEach((gallery)=>{if (gallery.drawingType==drawingType) returnGallery=gallery});
        return returnGallery;
    }
}

export default Galleries
