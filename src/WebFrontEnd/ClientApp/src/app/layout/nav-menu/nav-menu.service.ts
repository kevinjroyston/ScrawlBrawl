import { Injectable } from '@angular/core'
import { LayoutService } from '@layout/layout.service'

@Injectable()
export class NavMenuService extends LayoutService{
    constructor() {
        super()
        this.visible = true;
    }
}