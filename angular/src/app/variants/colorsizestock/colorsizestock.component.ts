import { Component, OnInit, Input, OnChanges } from '@angular/core';

@Component({
  selector: 'colorsizestock',
  providers: [
  ],
  styleUrls: ['./colorsizestock.component.css'],
  templateUrl: './colorsizestock.component.html'

})
export class ColorSizeStockComponent implements OnInit, OnChanges {

  public selectedSize: string = '';
  public remainingQty: string;
  public isDataLoaded: boolean;

  private selectColor: string = "Select a Colour";
  private selectSize: string = "Select a Size";

  public quantity: number = 1;
  
  @Input() selectedVariant: any;

  ngOnInit() {
    this.isDataLoaded = true;   
  }

  ngOnChanges() {
    if (this.selectedSize != '')
      this.checked(this.selectedSize);
    if (this.remainingQty == this.selectColor  && this.selectedVariant) {
      this.checked(this.selectedSize);
    }
    this.quantity =1;
  }
  isAvailable(size: any) {
    if (this.selectedVariant) {
      let index = this.selectedVariant.variants.findIndex(myObj => myObj['product_Size'] == size);
      if (index >= 0) {
        return false;
      }
      else {
        return true;
      }
    }
  }
  checked(size: string) {
    this.selectedSize = size;
    if (this.selectedVariant) {
      let index = this.selectedVariant.variants.findIndex(myObj => myObj['product_Size'] == size);
      if (index < 0) {
        this.selectedSize = null;
        this.remainingQty = this.selectSize;
      }
      else {
        this.remainingQty = this.selectedVariant.variants[index].product_Stock;
      }
    }
    else {
      this.remainingQty = this.selectColor;
    }
    this.quantity = 1;
  }
  addOne(){
    if(this.quantity < 10 && this.quantity < Number(this.remainingQty))
    this.quantity++;
  }
    reduceOne(){
    if(this.quantity > 1) 
    this.quantity--;
  }
}
