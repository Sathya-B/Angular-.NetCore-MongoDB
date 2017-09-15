import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import { ApiService } from '../../../services/api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AppState } from '../../app.service';
import {ToastMsgService} from '../../../services/toastmsg.service';
import { authServer } from '../../config/configuration';

@Component({
  selector: 'verification',
  templateUrl: './verification.component.html',
  styleUrls: ['./verification.component.scss']
})
export class VerificationComponent {
  public PhoneNumber: string;
  public JWT: any;
  public action: string;
  public postUrl: string;
  constructor(private apiService: ApiService, private toastmsg: ToastMsgService, private activatedRoute: ActivatedRoute, private router: Router, public appState: AppState) {
    this.PhoneNumber = activatedRoute.snapshot.paramMap.get('PhoneNumber');
    this.action = activatedRoute.snapshot.paramMap.get('action');
  }

  onSubmit(form: NgForm) {
    
    if(this.action == "/createaccount"){
     this.postUrl = "/register/smsverification"
    }else if(this.action == "/forgotpassword"){
    this.postUrl = "/forgotpassword/smsverification"
    }
    const verificationCode = form.value;
    verificationCode.PhoneNumber = this.PhoneNumber;
    console.log(verificationCode);
    this.apiService.post(this.postUrl, verificationCode, undefined, authServer).then(
      (response: any) => {
        console.log(response);
        if(response.value == undefined){
          throw response.error;
        }
        if (response.value.code == 999) {
          this.JWT = response.value.data;
          localStorage.setItem("JWT", this.JWT);
          localStorage.setItem("UserName", verificationCode.PhoneNumber);
          this.appState.set('loggedIn', true);
          window.alert('Logged in');
          this.router.navigate(['/']);
        }
        else if (response.value.code == 201) {
          this.JWT = response.value.data;
          localStorage.setItem("JWT", this.JWT);
          localStorage.setItem("UserName", verificationCode.PhoneNumber);
          this.router.navigate(['/updatepassword', verificationCode.PhoneNumber]);
        }
      })
      .catch(
      (error: any) => {
        console.log(error);
        var errormsg = JSON.parse(error._body);
        if (errormsg.code == "404") {
          this.toastmsg.popToast("error","Error","User not Registered");          
          this.router.navigate(['/forgotpassword']);
        }
        else if (errormsg.code == "401") {
          this.toastmsg.popToast("error","Error","OTP expired. Please try again");                    
          this.router.navigate(['/createaccount']);
        }
        else if (errormsg.code == "400") {
          this.toastmsg.popToast("error","Error","Invalid OTP");                    
        }
      }
    )
  }
}
