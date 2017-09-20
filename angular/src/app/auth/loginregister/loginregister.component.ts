import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ApiService } from '../../../services/api.service';
import { CartService } from '../../../services/cart.service';
import { ToastMsgService } from '../../../services/toastmsg.service';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Params, Router, ParamMap } from '@angular/router';
import { AppState } from '../../app.service';
import { apiUrl } from '../../config/configuration';

@Component({
  selector: 'app-loginregister',
  templateUrl: './loginregister.component.html',
  styleUrls: ['./loginregister.component.css']
})
export class LoginRegisterComponent {

  constructor(private appState: AppState, private cartService: CartService,
              private apiService: ApiService, private route: ActivatedRoute,
              private router: Router, private toastmsg: ToastMsgService) {
  }

public  onSignin(form: NgForm) {

    const loginDetails = form.value;
    console.log(apiUrl.authServer);
    this.apiService.post('/login', loginDetails, undefined, apiUrl.authServer).then(
      (response: any) => {
        console.log(response);
        if (response.value === undefined) {
          throw response.error;
        }
        if (response.value.code === '999') {
          this.toastmsg.popToast('success', 'Success', 'Welcome!');
          localStorage.setItem('JWT', response.value.data);
          localStorage.setItem('FirstName', response.value.content.FirstName);
          localStorage.setItem('UserName', loginDetails.UserName);
          this.appState.set('loggedIn', true);
          this.cartService.getCartItems(loginDetails.UserName);
          this.router.navigate(['/']);
        }
      })
      .catch(
      (error: any) => {
        if (error.code === '400') {
          this.toastmsg.popToast('error', 'Error', 'Wrong Credentials. Please try again');
        }
      }
    );
  }
}
