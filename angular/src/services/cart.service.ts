import { Inject, Injectable, Optional } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { ApiService } from './api.service';
import { AppState } from '../app/app.service';
import * as CartModel from '../models/cart.model';
import 'rxjs/add/operator/map';

@Injectable()
export class CartService {

    public cartItems: CartModel.Cart = { listOfProducts: [] };
    constructor(private apiService: ApiService, private appState: AppState) {
    }
    public getCount() {
        return this.cartItems.listOfProducts.length;
    }

    public getCartItems(userName: string) {
        this.apiService.get('user/cart/' + userName, { useAuth: true }).then(
            (response: any) => {
                this.cartItems.listOfProducts = response.data;
                console.log(this.cartItems);
            })
            .catch((error: any) => {
                console.log(error);
            });
    }
    public refreshCart() {
        let userName = localStorage.getItem('UserName');
        if (userName !== undefined) {
            this.apiService.put('user/cart/' + userName, this.cartItems , { useAuth: true }).then(
                (response: any) => {
                    console.log(response);
                })
                .catch((error: any) => {
                    console.log(error);
                });
        }
    }
    public removeFromCart(product: any) {

    }
    public updateCart(product: any, quantity: number) {

    }
}
