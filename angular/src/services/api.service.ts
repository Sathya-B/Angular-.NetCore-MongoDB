import { Inject, Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Rx';
import { serverUrl } from '../app/config/configuration';

@Injectable()
export class ApiService {

    private http: Http;
    private auth_token: string;
    public serverUrl = serverUrl;

    constructor( @Inject(Http) http: Http, private router: Router) {
        this.http = http;
    }

    header(valid_401?:boolean) {
        const headers = new Headers();

        return headers;
    }

    get(url: string, options?: any) {

        return this.http.get(serverUrl + url, {
            headers: this.header()
        }).timeout(30000)
            .map((res) => res.json())
            .do(res => {
                return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    }
    put(url: string, data: any, options?: any) {

        let body: any = data;
        return this.http.put(serverUrl + url, body, {
            headers: this.header()
        }).timeout(30000)
            .map((res) => res.json())
            .do(res => {
                return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    }
    patch(url: string, data: any, options?: any) {

        let body: any = data;
        return this.http.patch(serverUrl + url, body, {
            headers: this.header()
        }).timeout(30000)
            .map((res) => res.json())
            .do(res => {
                return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    }
    post(url: string, data: any, options?: any, serverUrl?: string) {

        let body: any = data;
        return this.http.post(serverUrl + url, body, {
            headers: this.header()
        }).timeout(30000)
            .map((res) => res.json())
            .do(res => {
                return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    }
    deleteData(url: string, data: any, options?: any) {

        return this.http.delete(serverUrl + url, {
            headers: this.header(),
            body: data
        }).timeout(30000)
            .map((res) => res.json())
            .do(res => {
                return this.handleSuccess(res, options);
            })
            .catch((err) => {
                return this.handleError(err, options);
            });
    }
  
    private handleSuccess(response: any, options: any) {

        return response;
    }
    private handleError(error: any, options: any) {

        let errorMsg = error;
        let body = errorMsg.json();
        if (body && body.code === 1103) {
            this.header(true);
        } else if (navigator.onLine && body.message) {
            console.log("error");
        }
        return Observable.throw(errorMsg || 'Server error');
    }
}
