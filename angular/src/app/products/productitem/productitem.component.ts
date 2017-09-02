import { Component, Input, OnInit } from '@angular/core';
import { DataServ} from '../../../services/data.service';
import {Router} from '@angular/router';
import {ActivatedRoute} from '@angular/router';

@Component({
  selector: 'productitem',

  styleUrls: ['./productitem.component.css'],

  templateUrl: './productitem.component.html'
})
export class ProductItemComponent implements OnInit {

@Input() productitem: any;

design: string;
Price: string;
ImgUrl: string;
for: string;
type: string;

constructor(private dataServ: DataServ, private router: Router, private activatedRoute: ActivatedRoute){
      this.for = activatedRoute.snapshot.paramMap.get('productFor');
      this.type = activatedRoute.snapshot.paramMap.get('productType');

}

ngOnInit(){
this.design = this.productitem.product_Design;
this.Price = this.productitem.topItem.product_Price;
this.ImgUrl = this.productitem.topItem.minioObject_Url;
}

DesignClicked(){
    this.dataServ.UpdateVariants(this.productitem);
    localStorage.setItem(this.for + "-" + this.type + "-" +this.design, JSON.stringify(this.productitem))
    console.log(this.productitem);
//     this.router.navigate(['../variants'],{relativeTo: this.activatedRoute});
}
}
