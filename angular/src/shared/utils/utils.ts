
export function expiredJwt(token: string) {
  let base64Url = token.split('.')[1];
  let base64 = base64Url.replace('-', '+').replace('_', '/');
  let jwt = JSON.parse(window.atob(base64));
  let exp = jwt.exp * 1000;
  var current_time = new Date().getTime() / 1000;
  if (current_time > jwt.exp) {
    return true;
  }
  else {
    return false;
  }
}

export function checkOptions(options?: any) {
  if (options) {
    if (options.useAuth) {
      return true;
    }
  }
  else {
    return false;
  }
}