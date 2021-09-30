<template>
  <v-card class="login-container">
    <span class="login-title">
      FISHBOOK
    </span>
    <v-btn small block color="primary" class="my-1" @click="authDialog = true">
      Вход
    </v-btn>
    <v-btn block small color="success" @click="registerDialog = true">
      Регистрация
    </v-btn>

    <v-dialog
      v-model="authDialog"
      transition="dialog-bottom-transition"
      max-width="300"
      persistent
    >
      <template v-slot:default>
        <v-card>
          <v-toolbar color="primary" dark>Авторизация</v-toolbar>
          <v-card-text class="pt-4">
            <v-text-field
              filled
              dense
              v-model="email"
              label="Email"
              hide-details
              class="mb-2"
            ></v-text-field>

            <v-text-field
              :append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
              :rules="[rules.required, rules.password]"
              :type="showPassword ? 'text' : 'password'"
              @click:append="showPassword = !showPassword"
              counter
              required
              filled
              dense
              v-model="password"
              label="Пароль"
              hide-details
              class="mb-2"
            ></v-text-field>
          </v-card-text>
          <v-card-actions>
            <v-btn text @click="closeAuthDialog">Отмена</v-btn>
            <v-spacer></v-spacer>
            <v-btn
              text
              color="primary"
              :loading="loadingLogin || loading"
              @click="signIn"
              >Войти</v-btn
            >
          </v-card-actions>
        </v-card>
      </template>
    </v-dialog>

    <v-dialog
      v-model="registerDialog"
      transition="dialog-top-transition"
      max-width="300"
      persistent
    >
      <template v-slot:default>
        <v-card>
          <v-toolbar color="success" dark>Регистрация</v-toolbar>

          <v-card-text class="pt-4">
            <v-alert
              type="error"
              dense
              v-if="signUpError"
              v-html="signUpError"
              style="font-size:85%;"
            ></v-alert>

            <v-text-field
              filled
              dense
              v-model="email"
              label="Email *"
              :rules="[rules.required, rules.email]"
              required
              class="mb-2"
            ></v-text-field>

            <v-text-field
              filled
              dense
              v-model="displayName"
              label="Ваше имя *"
              :rules="[rules.required]"
              required
              class="mb-2"
            ></v-text-field>

            <v-text-field
              :append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
              :rules="[rules.required, rules.password]"
              :type="showPassword ? 'text' : 'password'"
              @click:append="showPassword = !showPassword"
              counter
              required
              name="password"
              filled
              dense
              v-model="password"
              label="Пароль *"
            ></v-text-field>
          </v-card-text>
          <v-card-actions>
            <v-btn text @click="closeRegisterDialog" :disabled="loadingSignUp"
              >Отмена</v-btn
            >
            <v-spacer></v-spacer>
            <v-btn text color="primary" :loading="loadingSignUp" @click="signUp"
              >Регистрация</v-btn
            >
          </v-card-actions>
        </v-card>
      </template>
    </v-dialog>
  </v-card>
</template>

<script>
import ApiService from "../services/api.service";
import TokenService from "../services/token.service";
import SiteUrl from "../settings/siteUrl.settings";

//import { setToken } from "../../helpers/jwt-helper";
//import { doFetch } from "../../helpers/fetch-helper";

export default {
  props: ["loading"],
  data: () => ({
    authDialog: false,

    email: "",

    password: "",
    showPassword: false,

    loadingLogin: false,

    registerDialog: false,
    displayName: "",
    loadingSignUp: false,
    signUpError: "",

    rules: {
      required: (value) => !!value || "Обязательное поле",
      password: (v) =>
        /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{6,}$/.test(
          v
        ) ||
        "Минимум 6 символов, минимум одна буква, одна цифра и один специальный символ",
      email: (value) => {
        const pattern = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return pattern.test(value) || "Недопустимый email";
      },
    },
  }),
  methods: {
    clearVariables() {
      this.email = "";
      this.password = "";
      this.displayName = "";
    },
    closeAuthDialog() {
      this.authDialog = false;
      this.clearVariables();
    },
    closeRegisterDialog() {
      this.registerDialog = false;
      this.clearVariables();
    },
    async signUp() {
      if (!this.signUpCheck()) return;

      let body = {
        email: this.email,
        displayName: this.displayName,
        password: this.password,
      };

      let r = await ApiService.post(SiteUrl.registration(), body);

      if (!r.error) {
        TokenService.setUser(r.data);
        location.replace("/");
      } else {
        alert(r.error);
      }
    },
    async signIn() {
      let body = {
        email: this.email,
        password: this.password,
      };

      let r = await ApiService.post(SiteUrl.login(), body);

      TokenService.removeUser();

      if (!r.error) {
        TokenService.setUser(r.data);
        location.replace("/");
      } else {
        alert(r.error);
      }
    },
    signUpCheck() {
      let errors = [];

      if (!this.displayName) errors.push("Имя - обязательное поле");

      if (!this.email) errors.push("Email - обязательное поле");

      if (!this.password) errors.push("Пароль - обязательное поле");

      this.signUpError = errors.join("<br/>");

      return errors.length === 0;
    },
  },
};
</script>

<style scoped>
.login-container {
  position: absolute;
  right: 10px;
  top: 10px;
  z-index: 100;
  width: 140px;
  padding: 0.25rem;
}

.login-title {
  font-weight: 500;
  font-size: 14px;
  text-align: center;
  display: block;
}
</style>
