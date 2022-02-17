<template>
  <v-app>
    <v-main>
      <login v-if="!authed" :loading="loginLoading"></login>

      <template v-if="authed">
        <file-uploader
          @uploaded="fetchPhotos"
          @change-map-point="mapPointHandler"
        ></file-uploader>
        <filter-panel :photos="photos" @filtered="setFilter"> </filter-panel>
      </template>

      <fishbook-map
        ref="map"
        :items="points"
        :show-center-point="mapPoint"
        @select="selectPhoto"
      ></fishbook-map>

      <photo-card
        :data="selectedPhoto"
        :show="showPhotoCard"
        :fishs="fishs"
        :loading="loadingPhotoCard"
        @close="closePhotoCard"
        @deleted="deletedSelectedPhoto"
        @updated="updateSelectedPhoto"
        @created="createCustomPoint"
      ></photo-card>
    </v-main>
  </v-app>
</template>

<script>
import map from "./components/map.vue";

import login from "./components/login";
import fileUploader from "./components/file-uploader";
import photoCard from "./components/photo-card";
import filterPanel from "./components/filter-panel";

import ApiService from "./services/api.service";
import PhotoService from "./services/photo.service";
import SiteUrl from "./settings/siteUrl.settings";

export default {
  name: "App",

  components: {
    "fishbook-map": map,
    login,
    fileUploader,
    photoCard,
    filterPanel,
  },
  created() {
    //this.fixMobileHeight();

    this.checkAuth();
    this.getCurrentGelocation();
  },
  mounted() {},
  computed: {
    points() {
      let items = [];
      if (this.photos.length > 0) {
        this.photos.forEach((photo) => {
          let passPhoto = false;

          if (this.filter.years && this.filter.years.length > 0)
            passPhoto =
              this.filter.years.indexOf(
                new Date(photo.dateTime).getFullYear()
              ) === -1;

          if (!passPhoto && this.filter.fishs && this.filter.fishs.length > 0)
            passPhoto = this.filter.fishs.indexOf(photo.fishName) === -1;

          if (
            !passPhoto &&
            this.filter.months &&
            this.filter.months.length > 0
          ) {
            passPhoto =
              this.filter.months.indexOf(
                new Date(photo.dateTime).getMonth()
              ) === -1;
          }

          if (!passPhoto)
            items.push({
              id: photo.id,
              coordinates: [Number(photo.longitude), Number(photo.latitude)],
              date: new Date(photo.dateTime).toLocaleDateString(),
              data: photo,
            });
        });
      }
      return items;
    },
    fishs() {
      let items = [];
      if (this.photos.length > 0) {
        this.photos.forEach((photo) => {
          if (items.indexOf(photo.fishName) < 0 && photo.fishName)
            items.push(photo.fishName);
        });
      }
      return items;
    },
  },
  data: () => ({
    loginLoading: false,
    selectedFiles: undefined,

    center: [50.0665774, 53.141024099999996],
    zoom: 11,
    rotation: 0,
    geolocPosition: undefined,

    selectedSrc: "",

    overlay: false,
    authed: false,
    user: {},
    photos: [],

    selectedPhoto: {},
    showPhotoCard: false,
    loadingPhotoCard: false,

    currentPosition: null,
    filter: {},

    customCreate: false,

    map: null,
    mapPoint: false,
  }),
  methods: {
    async createCustomPoint(item) {
      this.loadingPhotoCard = true;

      let r = await PhotoService.upload(item);

      if (r.error) {
        alert(r.error);
        return;
      }

      this.photos.push(r.data);

      this.closePhotoCard();
    },
    toggleCustomeCreate() {
      this.customCreate = !this.customCreate;
    },
    closePhotoCard() {
      this.loadingPhotoCard = false;
      this.customCreate = false;
      this.showPhotoCard = false;
    },
    addCustomPoint(coordinates) {
      this.selectedPhoto = {
        dateTime: new Date().toISOString().substr(0, 16),
        fishName: "",
        latitude: coordinates[1],
        longitude: coordinates[0],
        customCreate: this.customCreate,
      };

      this.showPhotoCard = true;
    },
    setFilter(filter) {
      this.filter = filter;
    },
    async updateSelectedPhoto(data) {
      this.loadingPhotoCard = true;

      let r = await PhotoService.update(this.selectedPhoto.id, data);

      if (r.error) {
        alert(r.error);
        return;
      }

      let itemIndex = this.photos.findIndex(
        (e) => e.id === this.selectedPhoto.id
      );

      if (itemIndex > -1) {
        this.photos[itemIndex].fishName = r.data.fishName;
        this.photos[itemIndex].dateTime = r.data.dateTime;
      }

      this.closePhotoCard();
    },
    async deletedSelectedPhoto() {
      this.loadingPhotoCard = true;

      let r = await PhotoService.delete(this.selectedPhoto.id);

      if (r.error) {
        alert(r.error);
        return;
      }

      let itemIndex = this.photos.findIndex(
        (e) => e.id === this.selectedPhoto.id
      );

      if (itemIndex > -1) {
        this.photos.splice(itemIndex, 1);
      }

      this.closePhotoCard();
    },
    getCurrentGelocation() {
      if (navigator.geolocation) {
        navigator.geolocation.watchPosition(
          (position) => {
            this.currentPosition = [
              position.coords.longitude,
              position.coords.latitude,
            ];
          },
          (e) => {
            console.log("Ошибка определения геопозиции:", e);
          }
        );

        // navigator.geolocation.getCurrentPosition((pos) => {
        //   this.currentPosition = [pos.coords.longitude, pos.coords.latitude];
        // });
      }
    },
    getPhotoThumbUrl(photo) {
      return SiteUrl.photos() + "/" + photo.id + "/thumb";
    },
    async fetchPhotos() {
      let r = await ApiService.get(SiteUrl.photos());

      if (!r.error) {
        this.photos = r.data;
      }
    },
    async checkAuth() {
      this.loginLoading = true;

      let r = await ApiService.get(SiteUrl.profile());

      if (!r.error) {
        this.authed = true;
        this.user = r.data;
        this.fetchPhotos();
      }

      this.loginLoading = false;
    },
    selectPhoto(id) {
      let point = this.points.find((e) => e.id === id);

      if (point) {
        this.selectedPhoto = point.data;
        this.showPhotoCard = true;
      }
    },
    mapPointHandler(e) {
      this.mapPoint = e;

      if (!e) {
        this.customCreate = true;
        this.addCustomPoint(this.$refs.map.mapCenter);
      }
    },
  },
};
</script>

<style>
html {
  overflow-y: hidden;
  height: 100%;
}

body {
  overflow-y: hidden;
  height: 100%;
}

.map-wrapper {
  height: 99vh;
  overflow-y: hidden;
  position: absolute;
}

.profile-wrapper {
  position: absolute;
  right: 5px;
  top: 5px;
  z-index: 100;
  padding: 0.25rem;
}

.btn-custom-point {
  bottom: 10px;
  right: 10px;
}
</style>
