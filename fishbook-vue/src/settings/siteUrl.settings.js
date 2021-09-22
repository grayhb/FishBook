class SiteUrl {
  baseUrl() {
    return location.origin.indexOf("localhost") > -1
      ? "http://localhost:5704/"
      : "./";
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
    return this.baseUrl() + "api/files/upload";
  }
}

export default new SiteUrl();
