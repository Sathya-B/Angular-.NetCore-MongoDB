import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { ApiService } from '../../services/api.service';
import 'rxjs/add/observable/of';

@Injectable()
export class VariantsResolver implements Resolve<any> {

    constructor(private apiService: ApiService) {

    }
    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        let variants: any;

        variants = JSON.parse(localStorage.getItem(route.paramMap.get('productFor') + '-' +
            route.paramMap.get('productType') + '-' +
            route.paramMap.get('productDesign')));
        if (variants === null) {
        return this.apiService.get('Product/' + route.paramMap.get('productFor') +
                '/' + route.paramMap.get('productType') +
                '/' + route.paramMap.get('productDesign')).then(
                (response: any) => {
                    let products = response.data;
                    variants = {};
                    variants.productDesign = route.paramMap.get('productDesign');
                    variants.topItem = products[0];
                    variants.variants = products;
                    return variants;
                },
                (error: any) => {
                    console.log(error);
                }
                );            
        } else {
            return variants;
        }
    }
}
