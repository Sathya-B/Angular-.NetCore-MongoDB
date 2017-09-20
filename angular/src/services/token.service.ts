import { Inject, Injectable, Injector } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams,
         HttpRequest, HttpResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { Observer } from 'rxjs/Observer';
import { apiUrl } from '../app/config/configuration';
import * as Util from '../shared/utils/utils';
import * as Config from '../app/config/configuration';
import 'rxjs/Rx';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/toPromise';

@Injectable()
export class TokenService {
    public authToken: any;
    private http: HttpClient;
    constructor(http: HttpClient, private router: Router) {
        this.http = http;
    }
public getAuthToken(useAuth?: boolean) {
        let promise =  new Promise((resolve, reject) => {
            let JWT = JSON.parse(localStorage.getItem('JWT'));
            if (JWT != null) {
                let token = JWT;
                let expired: boolean;
                expired = Util.expiredJwt(token.access_token);
                if (token && !expired) {
                    resolve(token.access_token);
                } else if (token && expired) {
                    console.log('token expired');
                    let userName = JSON.parse(localStorage.getItem('UserName'));
                    return this.http.get(apiUrl.tokenServer +
                    'token/auth?grant_type=refresh_token&client_id=' +
                    userName + '&refresh_token=' + token.refresh_token, {}).timeout(30000)
                    .map((res) => { return res['data']; } )
                        .subscribe((response) => {
                            console.log(response);
                            localStorage.setItem('JWT', (response));
                            return resolve(JSON.parse(response).access_token);
                        }, (err) => {
                                reject('problem');
                                this.router.navigate(['/loginregister']);
                            });
                }
            } else if (useAuth) {
                 reject('no JWt');
                 this.router.navigate(['/loginregister']);
            } else {
                resolve('');
            }
        });
        return promise;
    }
}
