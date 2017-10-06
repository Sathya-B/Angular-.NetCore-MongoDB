import { Component, ElementRef, AfterViewInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';
import { ToastMsgService } from '../../../../services/toastmsg.service';
import { LoginLogoutService } from '../../../../services/loginlogout.service';
import { apiUrl } from '../../../config/configuration';

declare const gapi: any;

@Component({
  selector: 'google-signin',
  templateUrl: './googlesignin.component.html'
})
export class GoogleSigninComponent implements AfterViewInit {

  private clientId: string = '737899914464-f2vheiv2t0a02b1los4jl7n774jl4krp.apps.googleusercontent.com';

  private scope = [
    'profile',
    'email',
    'https://www.googleapis.com/auth/plus.me',
    'https://www.googleapis.com/auth/contacts.readonly',
    'https://www.googleapis.com/auth/admin.directory.user.readonly'
  ].join(' ');

  constructor(private element: ElementRef, private apiService: ApiService,
              private loginLogout: LoginLogoutService, private toastmsg: ToastMsgService) {
  console.log('ElementRef: ', this.element);
}

  public auth2: any;
  public googleInit() {
    let that = this;
    gapi.load('auth2', function () {
      that.auth2 = gapi.auth2.init({
        client_id: that.clientId,
        cookiepolicy: 'single_host_origin',
        scope: that.scope
      });
      that.attachSignin(that.element.nativeElement.firstChild);
    });
  }
  public attachSignin(element) {
    let that = this;
    this.auth2.attachClickHandler(element, {},
      function (googleUser) {

        let profile = googleUser.getBasicProfile();
        console.log('Token || ' + googleUser.getAuthResponse().id_token);
        console.log('ID: ' + profile.getId());
        console.log('Name: ' + profile.getName());
        console.log('Image URL: ' + profile.getImageUrl());
        console.log('Email: ' + profile.getEmail());

        //YOUR CODE HERE
        let postLogin = { ID: profile.getId(), Email: profile.getEmail(), Token: googleUser.getAuthResponse().id_token };
        that.apiService.post('/externallogin/google', postLogin, undefined, apiUrl.authServer).then(
          (response: any) => {
            if (response.value === undefined) {
              throw response.error;
            }
            if (response.value.code === '999') {
            let loginModel = { accessToken: response.value.data, firstName: response.value.content.FirstName, userName: profile.getEmail()}
            that.loginLogout.Login(loginModel);          
            }
          })
          .catch(
          (error: any) => {
            if (error.code === '401' || error.code === '402') {
              that.toastmsg.popToast('error', 'Error', 'Please try again.');
            }
            if (error.code === '400') {
              that.toastmsg.popToast('error', 'Error', 'Please try again.');
            }
          }
          );
      }, function(error) {
    console.log(error);
    console.log(JSON.stringify(error, undefined, 2));
  });
}

ngAfterViewInit() {
  this.googleInit();
}
}