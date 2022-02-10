import Vue from "vue";
import App from "./App.vue";
//import mainLayout from "./layouts/main-layout"
import VueRouter from "vue-router";
import Vuex from "vuex";
import vuetify from "./plugins/vuetify";

//import VueLayers from "vuelayers";
//import "vuelayers/lib/style.css";

Vue.config.productionTip = false;

Vue.use(VueRouter);
Vue.use(Vuex);
//Vue.use(VueLayers);

new Vue({
  vuetify,
  render: (h) => h(App),
}).$mount("#app");
