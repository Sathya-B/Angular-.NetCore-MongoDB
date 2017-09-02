import { Component } from '@angular/core';
import {Router} from '@angular/router';

@Component({
  selector: 'header',  // <header></header>

  styleUrls: ['./header.component.css'],

  templateUrl: './header.component.html'
})
export class HeaderComponent {

  public menuOpened: boolean = false;

  constructor(private router: Router){

  }
  public openMenu() {
    console.log('Menu Icon clicked.');
    this.menuOpened = !this.menuOpened;
  }
  public LoginRegister(){
  this.router.navigate(['/loginregister']);
}

}
