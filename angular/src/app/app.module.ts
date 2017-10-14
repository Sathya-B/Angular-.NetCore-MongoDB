import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { HttpClientModule } from '@angular/common/http';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule, ApplicationRef } from '@angular/core';
import { removeNgStyles, createNewHosts,
         createInputTransfer } from '@angularclass/hmr';
import { RouterModule, PreloadAllModules } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ENV_PROVIDERS } from './environment';
import { ROUTES } from './app.routes';

import { ToasterModule, ToasterService, ToasterConfig } from 'angular2-toaster';
import { SpinnerModule } from 'angular-spinners';
import { SpinnerService } from 'angular-spinners';
import { SwiperModule } from 'ngx-swiper-wrapper';
import { SwiperConfigInterface } from 'ngx-swiper-wrapper';
import { LazyLoadImageModule } from 'ng-lazyload-image';
import { ScrollToModule } from 'ng2-scroll-to';

import { AppComponent } from '@app/app.component';
import { HomeComponent } from '@app/home';
import { HeaderComponent } from '@app/header';
import { FooterComponent } from '@app/footer';
import { AuthGuard } from '@app/auth/authguard/authguard';
import { LoginRegisterComponent } from '@app/auth/loginregister/loginregister.component';
import { GoogleSigninComponent } from '@app/auth/loginregister/google/googlesignin.component';
import { FaceBookSigninComponent } from '@app/auth/loginregister/facebook/facebooksignin.component';
import { GetEmailComponent } from '@app/auth/loginregister/facebook/getemail/getemail.component';
import { CreateAccountComponent } from '@app/auth/createaccount/createaccount.component';
import { CheckEmailComponent } from '@app/auth/createaccount/checkemail/checkemail.component';
import { VerifyEmailComponent } from '@app/auth/createaccount/verifyemail/verifyemail.component';
import { VerificationComponent } from '@app/auth/verification/verification.component';
import { ForgotPasswordComponent } from '@app/auth/forgotpassword/forgotpassword.component';
import { UpdatePasswordComponent } from '@app/auth/updatepassword/updatepassword.component';
import { ChangePasswordComponent } from '@app/auth/changepassword/changepassword.component';
import { OrderListComponent } from '@app/orderlist/orderlist.component';
import { OrderItemComponent } from '@app/orderlist/orderitem/orderitem.component';
import { ProductsComponent } from '@app/products/products.component';
import { ProductItemComponent } from '@app/products/productitem/productitem.component';
import { ColorSizeStockComponent } from '@app/variants/colorsizestock/colorsizestock.component';
import { VariantsComponent } from '@app/variants/variants.component';
import { VariantsResolver } from '@app/variants/variants.resolver';
import { OrderListResolver } from '@app/orderlist/orderlist.resolver';
import { RelatedComponent } from '@app/variants/related/related.component';
import { CartComponent } from '@app/cart/cart.component';
import { CartItemComponent } from '@app/cart/cartitem/cartitem.component';
import { OrderComponent } from '@app/cart/order/order.component';
import { CheckOutComponent } from '@app/checkout/checkout.component';
import { MyAccountComponent } from '@app/account/myaccount.component';
import { AddressComponent } from '@app/account/address/address.component';
import { AddressBookComponent } from '@app/checkout/addressbook/addressbook.component';
import { NewAddressComponent } from '@app/checkout/newaddress/newaddress.component';
import { AddressItemComponent } from '@app/checkout/addressbook/addressitem/addressitem.component';
import { PrimaryAddressComponent } from '@app/checkout/primaryaddress/primaryaddress.component';
import { WishListComponent } from '@app/wishlist/wishlist.component';
import { WishListItemComponent } from '@app/wishlist/wishlistitem/wishlistitem.component';
import { CategoryComponent } from '@app/home/category';
import { NoContentComponent } from '@app/no-content';
import { AboutComponent } from '@app/policies/about/about.component';
import { ContactComponent } from '@app/policies/contact/contact';
import { CancellationComponent } from '@app/policies/cancellation/cancellation';
import { DeliveryComponent } from '@app/policies/delivery/delivery';
import { DisclaimerComponent } from '@app/policies/disclaimer/disclaimer';
import { PrivacyComponent } from '@app/policies/privacy/privacy';
import { TermsComponent } from '@app/policies/termsandconditions/terms';
import { AddedToCartComponent } from '@app/message/addedtocart/addedtocart';

import { CartService } from '@services/cart.service';
import { AddressService } from '@services/address.service';
import { WishListService } from '@services/wishlist.service';
import { ApiService } from '@services/api.service';
import { TokenService } from '@services/token.service';
import { LoginLogoutService } from '@services/loginlogout.service';
import { AuthInterceptor } from '@shared/auth.interceptor';
import { ToastMsgService } from '@services/toastmsg.service';
import { FilterPipe } from '../pipes/filterpipe.component';
import { CarouselModule } from 'angular4-carousel';
import { AddedToWishListComponent } from '@app/message/addedtowishlist/addedtowishlist';
import { APP_RESOLVER_PROVIDERS } from '@app/app.resolver';
import { AppState, InternalStateType } from './app.service';

import '../styles/styles.scss';
import '../styles/headings.css';

// Application wide providers
const APP_PROVIDERS = [
  ...APP_RESOLVER_PROVIDERS,
  AppState
];

const SWIPER_CONFIG: SwiperConfigInterface = {
  direction: 'horizontal'
};

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
    GoogleSigninComponent,
    FaceBookSigninComponent,
    GetEmailComponent,
    CreateAccountComponent,
    CheckEmailComponent,
    VerifyEmailComponent,
    VerificationComponent,
    ForgotPasswordComponent,
    UpdatePasswordComponent,
    ChangePasswordComponent,
    OrderListComponent,
    OrderItemComponent,
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
    MyAccountComponent,
    AddressComponent,
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
    FilterPipe
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
    LazyLoadImageModule,
    ScrollToModule.forRoot(),
    SwiperModule.forRoot(SWIPER_CONFIG),
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
    VariantsResolver,
    OrderListResolver,
    LoginLogoutService,
    AuthGuard,
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
