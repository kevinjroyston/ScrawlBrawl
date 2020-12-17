namespace Galleries  {

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

    export const galleryTypes:ReadonlyArray<GalleryType> = 
     [
        {galleryId:"HEAD",
        galleryDesc: "Heads",
        imageWidth: 360,
        imageHeight: 240,
        maxLocalFavorites: 6,
        maxLocalRecent: 4,    },

        {galleryId:"BODY",
        galleryDesc: "Bodies",
        imageWidth: 360,
        imageHeight: 240,
        maxLocalFavorites: 6,
        maxLocalRecent: 4,    },

        {galleryId:"LEGS",
        galleryDesc: "Legs",
        imageWidth: 360,
        imageHeight: 240,
        maxLocalFavorites: 6,
        maxLocalRecent: 4,    },

        {galleryId:"PROFILE",
        galleryDesc: "Profile Pictures",
        imageWidth: 300,
        imageHeight: 300,
        maxLocalFavorites: 4,
        maxLocalRecent: 3,    },

        {galleryId:"GENERIC",
        galleryDesc: "Images",
        imageWidth: 300,
        imageHeight: 300,
        maxLocalFavorites: 4,
        maxLocalRecent: 3,    },
    ];
    export function galleryFromId(galleryId):GalleryType{
        var returnGallery = galleryTypes[0];
        galleryTypes.forEach((gallery)=>{if (gallery.galleryId==galleryId) returnGallery=gallery});
        return returnGallery;
    }
}

export default Galleries
