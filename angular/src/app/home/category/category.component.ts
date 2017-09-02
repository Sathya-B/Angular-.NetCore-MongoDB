import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'category',

  styleUrls: ['./category.component.css'],

  templateUrl: './category.component.html'
})
export class CategoryComponent implements OnInit {

@Input() category: any;

Title: string;
Description: string;
ImgUrl: string;

constructor(){

}

ngOnInit(){
this.Title = this.category.product_For + " " + this.category.product_Type;
this.Description = this.category.description;
this.ImgUrl = this.category.minioObject_URL;
}

}
