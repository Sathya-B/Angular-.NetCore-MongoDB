import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import {ApiService} from '../../../services/api.service';
import {ActivatedRoute, Router} from '@angular/router';
import { AppState } from '../../app.service';

@Component({
  selector: 'changepassword',
  templateUrl: './changepassword.component.html',
  styleUrls: ['./changepassword.component.css']
})
export class ChangePasswordComponent {

constructor(private apiService: ApiService, private activatedRoute: ActivatedRoute, private router: Router,  public appState: AppState){

}

onSubmit(form: NgForm){
    const changePassword = form.value;
    console.log(changePassword);
    this.apiService.post('changepassword',changePassword,undefined,"http://192.168.0.111:5001/api/auth/").subscribe(
      (response: any) => {
          console.log(response);
        if(response.code == "200")
        {
         window.alert('Password Changed');
         this.router.navigate(['/']);
        }
      },
      (error: any) => {
        console.log(error);
        var errormsg = JSON.parse(error._body);
                if (errormsg.code == "400") {
          window.alert("Server Error. Please try again");        
        }
        else if (errormsg.code == "404") {
          window.alert("User not Found. Please Check Phone Number");
        }
        else if (errormsg.code == "401") {
          window.alert("Wrong Password.");
        }
      }
    )
}
}