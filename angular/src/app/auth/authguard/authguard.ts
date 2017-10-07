import { Injectable } from '@angular/core';
import { Router, CanActivate } from '@angular/router';
import * as Util from '../../../shared/utils/utils';

@Injectable()
export class AuthGuard implements CanActivate {
  constructor(private router: Router) { }
public canActivate() {
    if (JSON.parse(localStorage.getItem('JWT')) != null) {
      let jwt = JSON.parse(localStorage.getItem('JWT')).access_token;
      if (!Util.expiredJwt(jwt)) {
        console.log('true');
        return true;
      }
    }
    console.log('false');
    this.router.navigate(['/loginregister']);
    return false;
  }
}
