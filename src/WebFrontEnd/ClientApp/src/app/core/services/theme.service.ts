import { Injectable } from "@angular/core";
import { StorageService } from './storage.service'
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class ThemeService {

    private darkMode = false;
    private readonly THEME_KEY = 'theme';
    private readonly DARK_THEME_KEY = 'dark';
    private readonly LIGHT_THEME_KEY = 'light';
    private readonly DARK_THEME_CLASS = 'theme-dark';

    private themeSubject: BehaviorSubject<boolean>;
    public theme$: Observable<boolean>;

    constructor(private storage: StorageService){
        this.themeSubject = new BehaviorSubject<boolean>(this.isDarkMode());
        this.theme$ = this.themeSubject.asObservable();
    }

    public setThemeInit = () => {
        if (this.isDarkMode()){
            this.setDarkMode()
        } else {
            this.setLightMode()
        }
        document.body.classList.add('animate-colors-transition');
        
    }

    public isDarkMode = () : boolean => {
        this.darkMode = this.storage.get(this.THEME_KEY) === this.DARK_THEME_KEY;
        return this.darkMode;
    }

    public toggleTheme = () => {
        if (this.darkMode){
            this.setLightMode();
        } else {
            this.setDarkMode();
        }
        this.themeSubject.next(this.darkMode);
    }

    private setDarkMode = () => {
        this.storage.set(this.THEME_KEY, this.DARK_THEME_KEY);
        document.body.classList.add(this.DARK_THEME_CLASS);
        this.darkMode = true;
    }

    private setLightMode = () => {
        this.storage.set(this.THEME_KEY, this.LIGHT_THEME_KEY);
        document.body.classList.remove(this.DARK_THEME_CLASS);
        this.darkMode = false;
    }
}