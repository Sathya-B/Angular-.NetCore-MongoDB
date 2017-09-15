import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import {ApiService} from '../../../services/api.service';
import {ActivatedRoute, Router} from '@angular/router';
import { AppState } from '../../app.service';
import {ToastMsgService} from '../../../services/toastmsg.service';
import { authServer } from '../../config/configuration';

@Component({
  selector: 'changepassword',
  templateUrl: './changepassword.component.html',
  styleUrls: ['./changepassword.component.css']
})
export class ChangePasswordComponent {

constructor(private apiService: ApiService, private activatedRoute: ActivatedRoute, private router: Router, private toastmsg: ToastMsgService,  public appState: AppState){

}

onSubmit(form: NgForm){
    const changePassword = form.value;
    console.log(changePassword);
    this.apiService.post('changepassword',changePassword,undefined,authServer).then(
      (response: any) => {
          console.log(response);
        if(response.code == "200")
        {
         this.toastmsg.popToast("success","Success","Password Changed.")    
         this.router.navigate(['/']);
        }
      })
      .catch(
      (error: any) => {
        console.log(error);
        var errormsg = JSON.parse(error._body);
        if (errormsg.code == "400") {
        this.toastmsg.popToast("error","Error","Server Error. Please try again")          
        }
        else if (errormsg.code == "404") {
        this.toastmsg.popToast("error","Error","User not Found. Please Check Phone Number")          
        }
        else if (errormsg.code == "401") {
        this.toastmsg.popToast("error","Error","Wrong Password.")                  
        }
      }
    )
}
}