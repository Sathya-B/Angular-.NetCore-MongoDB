import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { Injectable, Injector } from '@angular/core';
import * as Util from '../shared/utils/utils';
import { Http, Headers } from '@angular/http';
import { TokenService } from '../services/token.service';


@Injectable()
export class AuthInterceptor implements HttpInterceptor {

    private auth_token: any;
    constructor(private inj: Injector) {

    }
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        console.log("intercepted");
        return next.handle(req);
    }
}

