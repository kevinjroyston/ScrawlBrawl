import {Subscription, Subject} from 'rxjs'
import {Injectable, OnDestroy} from '@angular/core'
import {MatLegacySnackBar as MatSnackBar, MatLegacySnackBarConfig as MatSnackBarConfig, MatLegacySnackBarRef as MatSnackBarRef, LegacySimpleSnackBar as SimpleSnackBar} from '@angular/material/legacy-snack-bar';

export class SnackBarMessage {
    message: string
    action: string
    config: MatSnackBarConfig
    horizontalPosition: "start" | "center" | "end" | "left" | "right"
    verticalPosition: "top" | "bottom"

    constructor(message: string, action : string, config: MatSnackBarConfig) {
        this.message = message;
        this.action = action;
        this.config = this.configureConfig(config);
    }

    configureConfig = (config : MatSnackBarConfig) => {
        if (config && !config.duration){
            config.duration = 2500;
        }
        config.horizontalPosition = config.horizontalPosition || "right";
        config.verticalPosition = config.verticalPosition || "bottom";
        return config
    }
}

@Injectable()
export class NotificationService implements OnDestroy {
    private messageQueue: Subject<SnackBarMessage> = new Subject<SnackBarMessage>();
    private subscription: Subscription;
    private snackBarRef: MatSnackBarRef<SimpleSnackBar>;
    private defaultConfig : MatSnackBarConfig = this.constructDefaultConfig();
    
    constructor(public _snackBar: MatSnackBar){
        this.subscription = this.messageQueue.subscribe(snackBarMessage => {
            this.snackBarRef = this._snackBar.open(
                snackBarMessage.message, snackBarMessage.action, snackBarMessage.config
            )
        });
    }

    private constructDefaultConfig() {
        let config = new MatSnackBarConfig();
        config.duration = 3000;
        return config;
    }

    ngOnDestroy(){
        this.subscription.unsubscribe();
    }

    addMessage = (message: string, action? : string, config: MatSnackBarConfig = this.defaultConfig) => {
        let snackBarMessage = new SnackBarMessage(message, action, config);
        this.messageQueue.next(snackBarMessage);
    }
}