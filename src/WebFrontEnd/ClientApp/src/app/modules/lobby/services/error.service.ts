import {Injectable} from '@angular/core'
import {Subject} from 'rxjs'

@Injectable()
export class ErrorService {
    public errorObservable = new Subject<string>();

    announceError = (error) => {
        this.errorObservable.next(error)
    }
}