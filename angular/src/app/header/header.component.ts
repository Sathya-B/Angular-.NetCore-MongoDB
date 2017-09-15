import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AppState } from '../app.service';
import { NgZone } from '@angular/core';
import {ToastMsgService} from '../../services/toastmsg.service';

@Component({
  selector: 'header',  // <header></header>

  styleUrls: ['./header.component.css'],

  templateUrl: './header.component.html'
})
export class HeaderComponent {

  public menuOpened: boolean = false;
  public scrolled: boolean = false;
  public loggedIn: { "loggedIn": boolean } = { loggedIn: false};
  constructor(private router: Router, public appState: AppState, zone: NgZone, private toastmsg: ToastMsgService) {
    this.loggedIn = this.appState.get('loggedIn');
    window.onscroll = () => {
      zone.run(() => {
        if(window.pageYOffset > 0) {
             this.scrolled = true;
        } else {
             this.scrolled = false;
        }
      });
    }

  }
  public openMenu() {
    console.log('Menu Icon clicked.');
    this.menuOpened = !this.menuOpened;
  }
  public LoginRegister() {   
    if (this.appState.get('loggedIn') != true)
     {
      this.router.navigate(['/loginregister']);
    }
  }

  ChangePassword(){
        this.router.navigate(['/changepassword']);  
  }
  SignOutClicked() {
    localStorage.removeItem("JWT");
    this.appState.set('loggedIn', false);
    this.loggedIn.loggedIn = false;
    this.toastmsg.popToast("success","Success","Logged Out");      
    this.router.navigate(['/']);    
  }

}
