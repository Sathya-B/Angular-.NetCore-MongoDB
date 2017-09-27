import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { HttpClientModule } from '@angular/common/http';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule, ApplicationRef } from '@angular/core';
import { removeNgStyles, createNewHosts, createInputTransfer } from '@angularclass/hmr';
import { RouterModule, PreloadAllModules } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ENV_PROVIDERS } from './environment';
import { ROUTES } from './app.routes';
// App is our top level component
import { AppComponent } from './app.component';
import { APP_RESOLVER_PROVIDERS } from './app.resolver';
import { AppState, InternalStateType } from './app.service';
import { HomeComponent } from './home';
import { HeaderComponent } from './header';
import { FooterComponent } from './footer';
import { LoginRegisterComponent } from './auth/loginregister/loginregister.component';
import { CreateAccountComponent } from './auth/createaccount/createaccount.component';
import { CheckEmailComponent } from './auth/createaccount/checkemail/checkemail.component';
import { VerifyEmailComponent } from './auth/createaccount/verifyemail/verifyemail.component';
import { VerificationComponent } from './auth/verification/verification.component';
import { ForgotPasswordComponent } from './auth/forgotpassword/forgotpassword.component';
import { UpdatePasswordComponent } from './auth/updatepassword/updatepassword.component';
import { ChangePasswordComponent } from './auth/changepassword/changepassword.component';
import { ProductsComponent } from './products/products.component';
import { ProductItemComponent } from './products/productitem/productitem.component';
import { ColorSizeStockComponent } from './variants/colorsizestock/colorsizestock.component';
import { VariantsComponent } from './variants/variants.component';
import { RelatedComponent } from './variants/related/related.component';
import { CartComponent } from './cart/cart.component';
import { CartItemComponent } from './cart/cartitem/cartitem.component';
import { OrderComponent } from './cart/order/order.component';
import { CheckOutComponent } from './checkout/checkout.component';
import { AddressBookComponent } from './checkout/addressbook/addressbook.component';
import { NewAddressComponent } from './checkout/newaddress/newaddress.component';
import { AddressItemComponent } from './checkout/addressbook/addressitem/addressitem.component';  
import { PrimaryAddressComponent} from './checkout/primaryaddress/primaryaddress.component';
import { WishListComponent } from './wishlist/wishlist.component';
import { WishListItemComponent } from './wishlist/wishlistitem/wishlistitem.component';
import { CartService } from '../services/cart.service';
import { AddressService } from '../services/address.service';
import { WishListService } from '../services/wishlist.service';
import { ApiService } from '../services/api.service';
import { TokenService } from '../services/token.service';
import { AuthInterceptor } from '../shared/auth.interceptor';
import { ToastMsgService } from '../services/toastmsg.service';
import { FilterPipe } from '../pipes/filterpipe.component';
import { CategoryComponent } from './home/category';
import { NoContentComponent } from './no-content';
import { XLargeDirective } from './home/x-large';
import { CarouselModule } from 'angular4-carousel';
import { ToasterModule, ToasterService, ToasterConfig } from 'angular2-toaster';
import { AboutComponent } from './policies/about/about.component';
import { ContactComponent } from './policies/contact/contact';
import { CancellationComponent } from './policies/cancellation/cancellation';
import { DeliveryComponent } from './policies/delivery/delivery';
import { DisclaimerComponent } from './policies/disclaimer/disclaimer';
import { PrivacyComponent } from './policies/privacy/privacy';
import { TermsComponent } from './policies/termsandconditions/terms';
import { AddedToCartComponent } from './message/addedtocart/addedtocart';
import { AddedToWishListComponent } from './message/addedtowishlist/addedtowishlist';
import { SpinnerModule } from 'angular-spinners';
import { SpinnerService } from 'angular-spinners';
import '../styles/styles.scss';
import '../styles/headings.css';

// Application wide providers
const APP_PROVIDERS = [
  ...APP_RESOLVER_PROVIDERS,
  AppState
];

type StoreType = {
  state: InternalStateType,
  restoreInputValues: () => void,
  disposeOldHosts: () => void
};

/**
 * `AppModule` is the main entry point into Angular2's bootstraping process
 */
@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    AppComponent,
    AboutComponent,
    HomeComponent,
    ProductsComponent,
    ProductItemComponent,
    VariantsComponent,
    CategoryComponent,
    LoginRegisterComponent,
    CreateAccountComponent,
    CheckEmailComponent,
    VerifyEmailComponent,
    VerificationComponent,
    ForgotPasswordComponent,
    UpdatePasswordComponent,
    ChangePasswordComponent,
    ColorSizeStockComponent,
    ContactComponent,
    CancellationComponent,
    DeliveryComponent,
    DisclaimerComponent,
    PrivacyComponent,
    TermsComponent,
    CartComponent,
    CartItemComponent,
    OrderComponent,
    CheckOutComponent,
    NewAddressComponent,
    AddressItemComponent,
    AddressBookComponent,
    PrimaryAddressComponent,
    WishListComponent,
    WishListItemComponent,
    RelatedComponent,
    HeaderComponent,
    FooterComponent,
    AddedToCartComponent,
    AddedToWishListComponent,
    NoContentComponent,
    FilterPipe,
    XLargeDirective
  ],
  /**
   * Import Angular's modules.
   */
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    HttpClientModule,
    CarouselModule,
    ToasterModule,
    SpinnerModule,
    BrowserAnimationsModule,
    RouterModule.forRoot(ROUTES, { useHash: true, preloadingStrategy: PreloadAllModules })
  ],
  /**
   * Expose our Services and Providers into Angular's dependency injection.
   */
  providers: [
    ENV_PROVIDERS,
    APP_PROVIDERS,
    CartService,
    AddressService,
    WishListService,
    ApiService,
    TokenService,
    ToastMsgService,
    SpinnerService,
    {provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true}
  ]
})
export class AppModule {

  constructor(
    public appRef: ApplicationRef,
    public appState: AppState
  ) { }

  public hmrOnInit(store: StoreType) {
    if (!store || !store.state) {
      return;
    }
    console.log('HMR store', JSON.stringify(store, null, 2));
    /**
     * Set state
     */
    this.appState._state = store.state;
    /**
     * Set input values
     */
    if ('restoreInputValues' in store) {
      let restoreInputValues = store.restoreInputValues;
      setTimeout(restoreInputValues);
    }

    this.appRef.tick();
    delete store.state;
    delete store.restoreInputValues;
  }

  public hmrOnDestroy(store: StoreType) {
    const cmpLocation = this.appRef.components.map((cmp) => cmp.location.nativeElement);
    /**
     * Save state
     */
    const state = this.appState._state;
    store.state = state;
    /**
     * Recreate root elements
     */
    store.disposeOldHosts = createNewHosts(cmpLocation);
    /**
     * Save input values
     */
    store.restoreInputValues = createInputTransfer();
    /**
     * Remove styles
     */
    removeNgStyles();
  }

  public hmrAfterDestroy(store: StoreType) {
    /**
     * Display new elements
     */
    store.disposeOldHosts();
    delete store.disposeOldHosts;
  }

}
