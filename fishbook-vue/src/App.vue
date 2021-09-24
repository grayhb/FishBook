<template>
  <v-app>
    <v-main>
      <login v-if="!authed"></login>

      <!-- <div v-else class="profile-wrapper">
        <v-chip label>{{ user.displayName }}</v-chip>
      </div> -->

      <file-uploader v-if="authed" @uploaded="fetchPhotos"></file-uploader>

      <filter-panel v-if="authed" :photos="photos" @filtered="setFilter">
      </filter-panel>

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
            <vl-style-circle :radius="6">
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
            <vl-style-circle :radius="10">
              <vl-style-fill color="green"></vl-style-fill>
              <vl-style-stroke color="green"></vl-style-stroke>
            </vl-style-circle>
          </vl-style-box>
        </vl-feature>

        <vl-interaction-select :features.sync="selectedFeatures">
        </vl-interaction-select>
      </vl-map>

      <photo-card
        :data="selectedPhoto"
        :show="showPhotoCard"
        :fishs="fishs"
        @close="showPhotoCard = false"
        @deleted="deletedSelectedPhoto"
        @updated="updateSelectedPhoto"
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
import SiteUrl from "./settings/siteUrl.settings";

export default {
  name: "App",

  components: {
    login,
    fileUploader,
    photoCard,
    filterPanel,
  },
  created() {
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

    currentPosition: null,
    filter: {},
  }),
  watch: {
    selectedFeatures(e) {
      this.getSourceByCoordinates(e[0]);
    },
  },
  methods: {
    setFilter(filter) {
      this.filter = filter;
    },

    async updateSelectedPhoto(data) {
      let url = `${SiteUrl.photos()}/${this.selectedPhoto.id}/update`;

      var r = await ApiService.put(url, data);

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

      this.showPhotoCard = false;
    },
    async deletedSelectedPhoto() {
      let url = `${SiteUrl.photos()}/${this.selectedPhoto.id}/delete`;

      let r = await ApiService.delete(url);
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

      this.showPhotoCard = false;
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
      let r = await ApiService.get(SiteUrl.profile());

      if (!r.error) {
        this.authed = true;
        this.user = r.data;
        this.fetchPhotos();
      }
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
html,
body {
  height: 100%;
  overflow-y: hidden;
}

.h-100 {
  height: 100%;
}

.profile-wrapper {
  position: absolute;
  right: 5px;
  top: 5px;
  z-index: 100;
  padding: 0.25rem;
}

.ol-zoom {
  display: none !important;
}
</style>
