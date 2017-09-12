import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import {ApiService} from '../../../services/api.service';
import {ActivatedRoute, Router} from '@angular/router';
import { AppState } from '../../app.service';

@Component({
  selector: 'updatepassword',
  templateUrl: './updatepassword.component.html',
  styleUrls: ['./updatepassword.component.css']
})
export class UpdatePasswordComponent {
public password: string;
public newPassword: { PhoneNumber: string, Password: string} = { PhoneNumber: "", Password:""};
public phoneNumber: string;
constructor(private apiService: ApiService, private activatedRoute: ActivatedRoute, private router: Router,  public appState: AppState){
    this.phoneNumber = activatedRoute.snapshot.paramMap.get('PhoneNumber');
}

onSubmit(form: NgForm){
    this.newPassword.Password = form.value.Password;
    this.newPassword.PhoneNumber = this.phoneNumber;  
    console.log(this.newPassword);
    this.apiService.post('forgotpassword/changepassword',this.newPassword,undefined,"http://192.168.0.111:5001/api/auth/").subscribe(
      (response: any) => {
          console.log(response);
        if(response.code == "200")
        {
         window.alert('Password Changed');
         this.appState.set('loggedIn', true);
         this.router.navigate(['/']);
        }
      },
      (error: any) => {
        console.log(error);
      }
    )
}
}