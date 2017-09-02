import { Component, OnInit, Output } from '@angular/core';
import { AppState } from '../app.service';
import { RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ParamMap } from '@angular/router';
import { DataServ} from '../../services/data.service';
import { EventEmitter } from '@angular/core';

@Component({
  selector: 'variants',  
  /**
   * We need to tell Angular's Dependency Injection which providers are in our app.
   */
  providers: [
  ],
  /**
   * Our list of styles in our component. We may add more to compose many styles together.
   */
  styleUrls: ['./variants.component.css'],
  /**
   * Every Angular template is first compiled by the browser before Angular runs it's compiler.
   */
  templateUrl: './variants.component.html'

})
export class VariantsComponent implements OnInit{
  /**
   * Set our default values
   */
  public localState = { value: '' };

  public variants: any;

  public design: string;

  public for: string;

  public type: string;

  public selectedColor: string;

  public selectedVariant: string;

  /**
   * TypeScript public modifiers
   */
  constructor(private dataServ: DataServ, private route: ActivatedRoute) {
    this.for = route.snapshot.paramMap.get('productFor');
    this.type = route.snapshot.paramMap.get('productType');
    this.design = route.snapshot.paramMap.get('productDesign');
  }

ngOnInit(){
this.variants = JSON.parse(localStorage.getItem(this.for + "-" + this.type + "-" + this.design));
 console.log(this.variants);
}

checked(event: any, svariant?: any){
  this.selectedColor = event.target.id;
  this.selectedVariant = svariant;
}
isChecked(color: string){
  if(this.selectedColor == color)
  {
    return true;
  }
  else{
    return false;
  }  
}
}
