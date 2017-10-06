import { Component, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import { ApiService } from '../../../../services/api.service';
import { ToastMsgService } from '../../../../services/toastmsg.service';
import { LoginLogoutService } from '../../../../services/loginlogout.service';
import { AppState } from '../../../app.service';
import { apiUrl } from '../../../config/configuration';

declare const FB: any;

@Component({
    selector: 'facebook-login',
    templateUrl: 'facebooksignin.component.html'
})

export class FaceBookSigninComponent implements OnInit {

    constructor(private router: Router, public apiService: ApiService,
        private appState: AppState, private loginLogout: LoginLogoutService) {
        FB.init({
            appId: '281046555724146',
            cookie: false,  // enable cookies to allow the server to access
            // the session
            xfbml: true,  // parse social plugins on this page
            version: 'v2.10' // use graph api version 2.1
        });
    }
    public fbResponse: any;
    onFacebookLoginClick() {
        // if(this.fbResponse !== null) {
        // if(this.fbResponse.authResponse != null) {
        // FB.logout(function(response) {
        //         document.location.reload();
        //     });
        // } else {
        // FB.login();            
        // }
        // } else {
        // FB.login();
        // }
        let that = this;
        FB.login(handlelogin);

        function handlelogin(loginresp) {
            console.log(loginresp);
            let postLogin = { ID: loginresp.authResponse.userID, Token: loginresp.authResponse.accessToken };
            that.apiService.post('/externallogin/facebook/check', postLogin, undefined, apiUrl.authServer).then(
                (response: any) => {
                    console.log(response)
                    if (response.value === undefined) {
                        throw response;
                    }
                    if (response.value.code === '999') {
                        let loginModel = { accessToken: response.value.data, firstName: response.value.content.FirstName, userName: response.value.content.UserName }
                        that.loginLogout.Login(loginModel);
                    }
                    console.log(response);
                })
                .catch(
                (error: any) => {
                    console.log(error);
                    if (error.code === '201') {
                        that.appState.set('registerFB', postLogin);
                        that.router.navigate(['./getemail']);
                    }
                }
                );
        }
    }

    statusChangeCallback(resp) {
        console.log('change');
        console.log(resp);
        if (resp.status === 'connected') {
            this.getUserFacebookProfile(resp.authResponse.accessToken);
            // connect here with your server for facebook login by passing access token given by facebook
        } else if (resp.status === 'not_authorized') {

        } else {

        }
    };
    ngOnInit() {
        FB.getLoginStatus(response => {
            this.fbResponse = response;
            console.log('Init');
            console.log(response);
            this.statusChangeCallback(response);
        });
    }
    getUserFacebookProfile(accessToken: string) {
        var fields = ['id', 'email', 'first_name', 'last_name', 'link', 'name', 'picture.type(small)'];
        var graphApiUrl = 'me?fields=' + fields.join(',');

        FB.api(graphApiUrl + '&access_token=' + accessToken + '', function (response) {
            console.log('api');
            console.log(response);
        });
    }
}