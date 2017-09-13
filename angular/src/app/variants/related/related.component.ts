import { Component, Input, OnInit, EventEmitter, Output } from '@angular/core';
import {Router} from '@angular/router';
import {ActivatedRoute} from '@angular/router';

@Component({
  selector: 'relateditem',

  styleUrls: ['./related.component.css'],

  templateUrl: './related.component.html'
})
export class RelatedComponent implements OnInit {

@Input() productitem: any;

design: string;
Price: string;
ImgUrl: string;
for: string;
type: string;
@Output() variantItemClicked = new EventEmitter<{}>();

constructor(private router: Router, private activatedRoute: ActivatedRoute){
      this.for = activatedRoute.snapshot.paramMap.get('productFor');
      this.type = activatedRoute.snapshot.paramMap.get('productType');
}

ngOnInit(){
this.design = this.productitem.product_Design;
this.Price = this.productitem.topItem.product_Price;
this.ImgUrl = this.productitem.topItem.minioObject_Url;
}

DesignClicked(){
    localStorage.setItem(this.for + "-" + this.type + "-" +this.design, JSON.stringify(this.productitem));
    this.variantItemClicked.emit(this.productitem);
  //  location.reload();
  //  this.router.navigateByUrl('/variants');
}
}
