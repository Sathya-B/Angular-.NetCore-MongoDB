import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpRequest, HttpResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Rx';
import { serverUrl } from '../app/config/configuration';
import * as Util from '../shared/utils/utils';
import {TokenService} from '../services/token.service';

import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/toPromise';

@Injectable()
export class ApiService {

    private http: HttpClient;
    private auth_token: string;
    public serverUrl = serverUrl;
    private tokenServ: TokenService;
    public category: any;

    constructor( http: HttpClient, private router: Router, private tokenService: TokenService) {
        this.http = http;
        this.tokenServ = tokenService;
    }

    header(token){
        const headers = new HttpHeaders().set('Content-Type','application/vnd.api+json')
        .set('Authorization', 'Bearer ' + token);
        return headers;
    }        
    get(url: string, options?: any) {
    var useAuth = Util.checkOptions(options);  
    return this.tokenService.getAuthToken(useAuth).then(       
        (token) => {
        const authHeader = this.header(token);
        return this.http.get(serverUrl + url, { headers: authHeader }).timeout(30000)
            .toPromise()
            .then((res) => {                
               return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    })
    }
    put(url: string, data: any, options?: any, serverUrl?: string) {
        var useAuth = Util.checkOptions(options);        
        let body: any = data;
        return this.tokenService.getAuthToken(useAuth).then(       
        (token) => {
        const authHeader = this.header(token);
        return this.http.post(serverUrl + url, body, { headers: authHeader }).timeout(30000)
            .toPromise()
            .then((res) => {                
               return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    })
    }
    patch(url: string, data: any, options?: any, serverUrl?: string) {
        var useAuth = Util.checkOptions(options);        
        let body: any = data;
        return this.tokenService.getAuthToken(useAuth).then(       
        (token) => {
        const authHeader = this.header(token);
        return this.http.post(serverUrl + url, body, { headers: authHeader }).timeout(30000)
            .toPromise()
            .then((res) => {                
               return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    })
    }
    post(url: string, data: any, options?: any, serverUrl?: string) {
        var useAuth = Util.checkOptions(options);        
        let body: any = data;
        return this.tokenService.getAuthToken(useAuth).then(       
        (token) => {
        const authHeader = this.header(token);
        return this.http.post(serverUrl + url, body, { headers: authHeader }).timeout(30000)
            .toPromise()
            .then((res) => {                
               return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    })
    }
    deleteo(url: string, data: any, options?: any, serverUrl?: string) {
        var useAuth = Util.checkOptions(options);        
        let body: any = data;
        return this.tokenService.getAuthToken(useAuth).then(       
        (token) => {
        const authHeader = this.header(token);
        return this.http.post(serverUrl + url, body, { headers: authHeader }).timeout(30000)
            .toPromise()
            .then((res) => {                
               return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    })
    }
    private handleSuccess(response: any, options: any) {
        return response;
    }
    private handleError(error: any, options: any) {
        return error;
    }
}
