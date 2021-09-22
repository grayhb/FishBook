class TokenService {
  getToken() {
    let user = localStorage.getItem("user");
    if (!user) return null;

    user = JSON.parse(user);
    return user.token;
  }

  getUser() {
    let user = localStorage.getItem("user");

    if (!user) return null;

    return JSON.parse(user);
  }

  setUser(user) {
    if (user) localStorage.setItem("user", JSON.stringify(user));
  }

  removeUser() {
    localStorage.removeItem("user");
  }
}

export default new TokenService();
