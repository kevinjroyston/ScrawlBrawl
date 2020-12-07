import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class StorageService {
    public get = (key: string): string => {
        return localStorage ? localStorage.getItem(key) : null;
    }
    
    public set = (key: string, value: any): void => {
        if (localStorage) {
            localStorage.setItem(key, value);
        }
    }
}