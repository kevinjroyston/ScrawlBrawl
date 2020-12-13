export namespace Galleries  {

    export const favorites: string = 'Favorites';
    export const recent: string = 'Recent';
    export const samples: string = 'Samples';

    export class GalleryType {
        galleryId: string;
        galleryDesc: string;
        imageWidth: number;
        imageHeight: number;
        maxLocalFavorites: number;
        maxLocalRecent: number;
    }

    export const galleryTypes:GalleryType[] = 
     [
        {galleryId:"HEAD",
        galleryDesc: "Heads",
        imageWidth: 360,
        imageHeight: 240,
        maxLocalFavorites: 10,
        maxLocalRecent: 10,    },

        {galleryId:"BODY",
        galleryDesc: "Bodies",
        imageWidth: 360,
        imageHeight: 240,
        maxLocalFavorites: 10,
        maxLocalRecent: 10,    },

        {galleryId:"LEGS",
        galleryDesc: "Legs",
        imageWidth: 360,
        imageHeight: 240,
        maxLocalFavorites: 10,
        maxLocalRecent: 10,    },

        {galleryId:"PROFILE",
        galleryDesc: "Profile Pictures",
        imageWidth: 300,
        imageHeight: 300,
        maxLocalFavorites: 10,
        maxLocalRecent: 10,    },
    ];
    export function galleryFromType(galleryType):GalleryType{
        var returnGallery = galleryTypes[0];
        galleryTypes.forEach((gallery)=>{if (gallery.galleryId==galleryType) returnGallery=gallery});
        return returnGallery;
    }
}

export default Galleries
