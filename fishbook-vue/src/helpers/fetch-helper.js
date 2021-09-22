export function doFetch(url, method, body, store, callback, errorCallback) {
  return fetch(url, {
    method: method,
    credentials: "include",
    accept: "application/json",
    body: body,
    headers: {
      Accept: "application/json",
      Authorization: "Bearer " + store.state.token
    }
  }).then(async response => {
    if (response.status === 200) callback(await response.json());
    else {
      let errorText = "";
      switch (response.status) {
        case 400: {
          let json = await response.json();
          errorText = json.error ?? "Ошибка выполнения операции";
          break;
        }
        case 401:
          errorText = "Ошибка авторизации";
          localStorage.removeItem("token");
          window.location.replace("/");
          break;
        case 403:
          errorText = "Доступ запрещен";
          break;
        case 404:
          errorText = "Страница или запись не найдена, повторите операцию";
          break;
        default:
          errorText = "Ошибка сервера, повторите операцию";
          break;
      }

      errorCallback(errorText);
    }
  });
}
