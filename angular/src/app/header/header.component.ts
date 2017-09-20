import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AppState } from '../app.service';
import { NgZone } from '@angular/core';
import { ToastMsgService } from '../../services/toastmsg.service';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'header',  // <header></header>

  styleUrls: ['./header.component.css'],

  templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit {

  public menuOpened: boolean = false;
  public scrolled: boolean = false;
  public loggedIn: { 'loggedIn': boolean } = { loggedIn: false};
  public wishlistCount: number = 0;
  public cartCount: number = 0;

  constructor(private router: Router, public appState: AppState,
              zone: NgZone, private toastmsg: ToastMsgService,
              private cartService: CartService) {
    this.loggedIn = this.appState.get('loggedIn');
    window.onscroll = () => {
      zone.run(() => {
      if (window.pageYOffset > 0) {
        this.scrolled = true;
        } else {
        this.scrolled = false;
        }
      });
  };
}
public ngOnInit() {
    if (this.appState.get('loggedIn') === true) {
    this.cartService.getCartItems(localStorage.getItem('UserName'));
    }
  }
public openMenu() {
    console.log('Menu Icon clicked.');
    this.menuOpened = !this.menuOpened;
  }
public LoginRegister() {
    if (this.appState.get('loggedIn') !== true) {
      this.router.navigate(['/loginregister']);
    }
  }

public ChangePassword() {
        this.router.navigate(['/changepassword']);
  }
public SignOutClicked() {
    localStorage.removeItem('JWT');
    this.appState.set('loggedIn', false);
    this.loggedIn.loggedIn = false;
    this.toastmsg.popToast('success', 'Success', 'Logged Out');
    this.cartService.cartItems.listOfProducts = [];
    this.router.navigate(['/']);
  }
public getUserName(){
  return localStorage.getItem('FirstName');
}
}
