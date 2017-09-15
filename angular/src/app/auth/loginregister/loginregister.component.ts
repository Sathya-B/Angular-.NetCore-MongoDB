import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ApiService } from '../../../services/api.service';
import { ToastMsgService } from '../../../services/toastmsg.service';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Params, Router, ParamMap } from '@angular/router';
import { AppState } from '../../app.service';
import { authServer } from '../../config/configuration';


@Component({
  selector: 'app-loginregister',
  templateUrl: './loginregister.component.html',
  styleUrls: ['./loginregister.component.css']
})
export class LoginRegisterComponent {

  constructor(private appState: AppState, private apiService: ApiService, private route: ActivatedRoute, private router: Router, private toastmsg: ToastMsgService) {

  }


  onSignin(form: NgForm) {

    const loginDetails = form.value;

    this.apiService.post('/login', loginDetails, undefined, authServer).then(
      (response: any) => {
        console.log(response);
        if(response.value == undefined){
          throw response.error;
        }
        if (response.value.code == "999") {
           this.toastmsg.popToast("success", "Success", "Welcome!")
          localStorage.setItem("JWT", response.value.data);
          localStorage.setItem("UserName", loginDetails.UserName);
          this.appState.set('loggedIn', true);
          this.router.navigate(['/']);
        }
      })
      .catch(
      (error: any) => {
        if (error.code == "400") {
          this.toastmsg.popToast("error", "Error", "Wrong Credentials. Please try again")
        }
      }
    )
  }
}

