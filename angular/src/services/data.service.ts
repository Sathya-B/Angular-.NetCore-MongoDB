import { Category } from '../models/category.model';
import { Inject,Injectable,Optional } from '@angular/core';
import { Observable }                                           from 'rxjs/Observable';
import 'rxjs/add/operator/map';

@Injectable()
export class DataServ{

public variants: any;


    constructor() {

    }

    UpdateVariants(variant: any){
        this.variants = variant;
    }

    GetVariants(){
        return this.variants;
    }

}