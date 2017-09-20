/**
 * Angular 2 decorators and services
 */
import {
  Component,
  OnInit,
  ViewEncapsulation
} from '@angular/core';
import { AppState } from './app.service';
import { Router, NavigationEnd } from '@angular/router';
import { ToastMsgService } from '../services/toastmsg.service';

/**
 * App Component
 * Top Level Component
 */
@Component({
  selector: 'app',
  encapsulation: ViewEncapsulation.None,
  styleUrls: [
    './app.component.css'
  ],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  public angularclassLogo = 'assets/img/angularclass-avatar.png';
  public name = 'Angular 2 Webpack Starter';
  public url = 'https://twitter.com/AngularClass';
  public config: any;
  constructor(public appState: AppState, private router: Router,
              private toastmsg: ToastMsgService) {
    this.config = toastmsg.config;
  }

  public ngOnInit() {
    if (localStorage.getItem('UserName') != null && localStorage.getItem('JWT') != null) {
      this.appState.set('loggedIn', true);
    } else {
      this.appState.set('loggedIn', false);
    }
    console.log('Initial App State', this.appState.state);

    this.router.events.subscribe((evt) => {
      if (!(evt instanceof NavigationEnd)) {
        return;
      }
      window.scrollTo(0, 0);
    });
  }

}
