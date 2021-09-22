export function setToken(token) {
    localStorage.setItem('token', token);
}

export function getToken() {
    let token = localStorage.getItem('token');
    return token;
}

export function deleteToken() {
    localStorage.removeItem('token'); 
}