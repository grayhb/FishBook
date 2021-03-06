class SiteUrl {
  baseUrl() {
    if (location.origin.indexOf("192.168.1.98") > -1)
      return "http://192.168.1.98:5704/";

    return location.origin.indexOf("localhost") > -1
      ? "http://localhost:5704/"
      : "./server/";
  }

  registration() {
    return this.baseUrl() + "registration";
  }

  login() {
    return this.baseUrl() + "login";
  }

  profile() {
    return this.baseUrl() + "api/user/profile";
  }

  upload() {
    return this.baseUrl() + "api/photos/create";
  }

  photos() {
    return this.baseUrl() + "api/photos";
  }
}

export default new SiteUrl();
