<template>
  <v-app>
    <v-main>
      <login v-if="!authed" :loading="loginLoading"></login>

      <template v-if="authed">
        <file-uploader @uploaded="fetchPhotos"></file-uploader>
        <filter-panel :photos="photos" @filtered="setFilter"> </filter-panel>
        <v-btn
          depressed
          absolute
          fab
          class="btn-custom-point"
          color="primary"
          @click="toggleCustomeCreate"
        >
          <v-icon v-if="!customCreate">mdi-map-marker</v-icon>
          <v-icon v-else>mdi-map-marker-off</v-icon>
        </v-btn>
      </template>

      <vl-map
        class="h-100"
        style="position:absolute"
        :load-tiles-while-animating="true"
        :load-tiles-while-interacting="true"
        data-projection="EPSG:4326"
      >
        <vl-view
          :zoom.sync="zoom"
          :center.sync="center"
          :rotation.sync="rotation"
        ></vl-view>

        <vl-layer-tile id="osm">
          <vl-source-osm></vl-source-osm>
        </vl-layer-tile>

        <vl-feature v-for="point in points" :key="point.id">
          <vl-geom-point :coordinates="point.coordinates"></vl-geom-point>

          <vl-style-box>
            <vl-style-circle :radius="8">
              <vl-style-fill color="#d7e8f5"></vl-style-fill>
              <vl-style-stroke color="#1f608f"></vl-style-stroke>
            </vl-style-circle>

            <vl-style-text
              :text="point.date"
              font="12px monospace"
              :offsetY="-14"
            ></vl-style-text>
          </vl-style-box>
        </vl-feature>

        <vl-feature v-if="currentPosition">
          <vl-geom-point :coordinates="currentPosition"></vl-geom-point>
          <vl-style-box>
            <vl-style-circle :radius="6">
              <vl-style-fill color="green"></vl-style-fill>
              <vl-style-stroke color="green"></vl-style-stroke>
            </vl-style-circle>
          </vl-style-box>
        </vl-feature>

        <vl-interaction-select :features.sync="selectedFeatures">
        </vl-interaction-select>

        <vl-layer-vector :z-index="1">
          <vl-source-vector
            :features.sync="features"
            ident="the-source"
          ></vl-source-vector>

          <vl-style-box>
            <vl-style-stroke color="green"></vl-style-stroke>
            <vl-style-fill color="rgba(255,255,255,0.5)"></vl-style-fill>
          </vl-style-box>
        </vl-layer-vector>

        <vl-interaction-draw
          v-if="customCreate"
          type="Point"
          source="the-source"
        >
          <vl-style-box>
            <vl-style-circle :radius="6">
              <vl-style-fill color="#d7e8f5"></vl-style-fill>
              <vl-style-stroke color="#1f608f"></vl-style-stroke>
            </vl-style-circle>
          </vl-style-box>
        </vl-interaction-draw>
      </vl-map>

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
import login from "./components/login";
import fileUploader from "./components/file-uploader";
import photoCard from "./components/photo-card";
import filterPanel from "./components/filter-panel";

import ApiService from "./services/api.service";
import PhotoService from "./services/photo.service";
import SiteUrl from "./settings/siteUrl.settings";

import "vuelayers/dist/vuelayers.css";

export default {
  name: "App",

  components: {
    login,
    fileUploader,
    photoCard,
    filterPanel,
  },
  created() {
    this.fixMobileHeight();

    this.checkAuth();
    this.getCurrentGelocation();
  },
  computed: {
    points() {
      let items = [];
      if (this.photos.length > 0) {
        this.photos.forEach((photo) => {
          let passPhoto = false;

          if (this.filter.years && this.filter.years.length > 0) {
            passPhoto =
              this.filter.years.indexOf(
                new Date(photo.dateTime).getFullYear()
              ) === -1;
          }

          if (!passPhoto && this.filter.fishs && this.filter.fishs.length > 0)
            passPhoto = this.filter.fishs.indexOf(photo.fishName) === -1;

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
    features: [],

    loginLoading: false,
    selectedFiles: undefined,

    center: [50.0665774, 53.141024099999996],
    zoom: 11,
    rotation: 0,
    geolocPosition: undefined,
    selectedFeatures: [],

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
  }),
  watch: {
    selectedFeatures(e) {
      this.getSourceByCoordinates(e[0]);
    },

    features(e) {
      if (e.length > 0) {
        this.addCustomPoint(e[0].geometry.coordinates);
        e.splice(0, 1);
      }
    },
  },
  methods: {
    fixMobileHeight() {
      // link - https://css-tricks.com/the-trick-to-viewport-units-on-mobile/
      // We listen to the resize event
      window.addEventListener("resize", () => {
        // We execute the same script as before
        let vh = window.innerHeight * 0.01;
        document.documentElement.style.setProperty("--vh", `${vh}px`);
      });
    },
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
      this.selectedFeatures = [];
      this.loadingPhotoCard = false;
      this.customCreate = false;
      this.showPhotoCard = false;
    },
    addCustomPoint(coordinates) {
      this.selectedPhoto = {
        dateTime: new Date().toISOString(),
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

    getSourceByCoordinates(feature) {
      let c = feature?.geometry?.coordinates;

      if (!c) return null;

      let f = 12;

      let point = this.points.find(
        (e) =>
          e.coordinates[0].toFixed(f) === c[0].toFixed(f) &&
          e.coordinates[1].toFixed(f) === c[1].toFixed(f)
      );

      if (point) {
        this.selectedPhoto = point.data;
        this.showPhotoCard = true;
      }
    },
  },
};
</script>

<style>
html {
  height: -webkit-fill-available;
}

body {
  overflow-y: hidden;
  min-height: 100vh;
  /* mobile viewport bug fix */
  min-height: -webkit-fill-available;
}

.h-100 {
  height: 100%;
  height: calc(var(--vh, 1vh) * 100);
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

.ol-zoom {
  display: none !important;
}
</style>
