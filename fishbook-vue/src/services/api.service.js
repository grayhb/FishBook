import TokenService from "./token.service";

class ApiService {
  async get(url) {
    return await this.sendRequest(url, "GET");
  }

  async post(url, body, isBlob = false, needAuth = true) {
    return await this.sendRequest(url, "POST", body, isBlob, needAuth);
  }

  async put(url, body) {
    return await this.sendRequest(url, "PUT", body);
  }

  async delete(url) {
    return await this.sendRequest(url, "DELETE");
  }

  async sendRequest(url, method, body, isBlob = false, needAuth = true) {
    let options = {
      method: method,
      credentials: "include",
      accept: "application/json",
      body: body instanceof FormData ? body : JSON.stringify(body),
      mode: "cors",
      headers: {
        Accept: "application/json",
      },
    };

    if (body !== null) {
      if (!(body instanceof FormData))
        options.headers["Content-Type"] = "application/json";
      // options.headers = {
      //   "Content-Type": "application/json",
      // };
    }

    let token = TokenService.getToken();
    if (token && needAuth) {
      //options.headers["Accept"] = "application/json";
      options.headers["Authorization"] = "Bearer " + token;
    }

    var fetchResponse = undefined;

    await fetch(url, options)
      .then(async (response) => {
        let responseError = undefined;
        let responseStatus = response.status;
        let responseData = null;

        if (response.status === 200)
          if (isBlob) responseData = await response.blob();
          else {
            try {
              responseData = await response.json();
            } catch (ex) {
              console.log(ex);
            }
          }
        else {
          switch (response.status) {
            case 204:
              break;
            case 400: {
              let json = await response.json();
              responseError = json.error
                ? json.error
                : "Ошибка выполнения операции";
              break;
            }
            case 401:
              responseError = "Ошибка авторизации";
              break;
            case 403:
              responseError = "Доступ запрещен";
              break;
            case 404:
              responseError =
                "Страница или запись не найдена, повторите операцию";
              break;
            default:
              responseError = "Ошибка сервера, повторите операцию";
              break;
          }
        }
        fetchResponse = {
          data: responseData,
          error: responseError,
          status: responseStatus,
        };
      })
      .catch((ex) => {
        let error = ex.toString();

        if (error === "TypeError: Failed to fetch") {
          error = "Отсутствует подключение к серверу";
        }

        fetchResponse = {
          data: null,
          error,
        };
      });

    return fetchResponse;
  }
}

export default new ApiService();
