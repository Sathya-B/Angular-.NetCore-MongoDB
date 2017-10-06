import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { CartService } from './cart.service';
import { WishListService } from './wishlist.service';
import { ToastMsgService } from './toastmsg.service';
import { AppState } from '../app/app.service';

@Injectable()
export class LoginLogoutService {
    constructor(private cartService: CartService, private wishListService: WishListService,
        private toastmsg: ToastMsgService, private appState: AppState,
        private router: Router) {
    }
    public Login(loginModel: any) {
        this.toastmsg.popToast('success', 'Success', 'Welcome!');
        localStorage.setItem('JWT', loginModel.accessToken);
        localStorage.setItem('FirstName', loginModel.firstName);
        localStorage.setItem('UserName', loginModel.userName);
        this.appState.set('loggedIn', true);
        this.cartService.getCartItems(loginModel.userName);
        this.wishListService.getWishListItems(loginModel.userName);
        this.router.navigate(['/']);
    }
    public Logout() {
        this.cartService.refreshCart().then((res) => {
            this.cartService.cartItems.listOfProducts = [];
            this.wishListService.refreshList().then((res) => {
                this.wishListService.wishListItems.listOfProducts = [];
                localStorage.removeItem('JWT');
            });
        });
        this.appState.set('loggedIn', false);
        this.toastmsg.popToast('success', 'Success', 'Logged Out');
        this.router.navigate(['/']);
    }
}
