import { Component, OnInit } from '@angular/core';
import { AppState } from '../app.service';
import { Title } from './title';
import { DataServ } from '../../services/data.service';
import {ApiService} from '../../services/api.service';
import { Category } from '../../models/category.model';
import { XLargeDirective } from './x-large';
import { CategoryComponent } from './category';
import { ICarouselConfig, AnimationConfig } from 'angular4-carousel';

@Component({
  /**
   * The selector is what angular internally uses
   * for `document.querySelectorAll(selector)` in our index.html
   * where, in this case, selector is the string 'home'.
   */
  selector: 'home',  // <home></home>
  /**
   * We need to tell Angular's Dependency Injection which providers are in our app.
   */
  providers: [
    Title
  ],
  /**
   * Our list of styles in our component. We may add more to compose many styles together.
   */
  styleUrls: ['./home.component.css'],
  /**
   * Every Angular template is first compiled by the browser before Angular runs it's compiler.
   */
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  /**
   * Set our default values
   */
  public localState = { value: '' };

  public imageSources: string[] = [
    "../assets/img/sliderhome/desktop/1200-X-450_B_01.jpg",
    "../assets/img/sliderhome/desktop/1200-X-450_B_02.jpg",
    "../assets/img/sliderhome/desktop/1200-X-450_B_03.jpg",
    "../assets/img/sliderhome/desktop/1200-X-450_B_04.jpg"
  ];

  public config: ICarouselConfig = {
    verifyBeforeLoad: false,
    log: false,
    animation: true,
    animationType: AnimationConfig.SLIDE,
    autoplay: false,
    autoplayDelay: 2000,
    stopAutoplayMinWidth: 1200
  };

  public category: any[] = [];

  /**
   * TypeScript public modifiers
   */
  constructor(public appState: AppState, public title: Title, private dataServ: DataServ, private apiService: ApiService) {
  }
  ngOnInit() {
    this.GetCategories();
  }

  GetCategories(){
    this.apiService.get('category').subscribe(
      (response: any) => {
        console.log(response);
        this.category = response.result;
      },
      (error: any) => {
        console.log(error);
      }
    )
  }
}
