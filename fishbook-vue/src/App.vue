<template>
  <v-app>
    <v-main>
      <login v-if="!authed"></login>

      <div v-else class="profile-wrapper">
        <v-chip>{{ user.displayName }}</v-chip>
      </div>

      <file-uploader v-if="authed" @uploaded="fetchPhotos"></file-uploader>
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

        <vl-geoloc @update:position="geolocPosition = $event">
          <template slot-scope="geoloc">
            <vl-feature v-if="geoloc.position" id="position-feature">
              <vl-geom-point :coordinates="geoloc.position"></vl-geom-point>
              <vl-style-box>
                <vl-style-icon
                  src="_media/marker.png"
                  :scale="0.4"
                  :anchor="[0.5, 1]"
                ></vl-style-icon>
              </vl-style-box>
            </vl-feature>
          </template>
        </vl-geoloc>

        <vl-layer-tile id="osm">
          <vl-source-osm></vl-source-osm>
        </vl-layer-tile>

        <vl-feature v-for="point in points" :key="point.id">
          <vl-geom-point :coordinates="point.coordinates"></vl-geom-point>

          <vl-style-box>
            <vl-style-circle :radius="5" style="cursor: pointer;">
              <vl-style-fill color="white"></vl-style-fill>
              <vl-style-stroke color="blue"></vl-style-stroke>
            </vl-style-circle>

            <vl-style-text
              :text="point.date"
              font="12px monospace"
              :offsetY="-14"
            ></vl-style-text>
          </vl-style-box>
        </vl-feature>

        <vl-interaction-select :features.sync="selectedFeatures">
        </vl-interaction-select>
      </vl-map>

      <photo-card
        :data="selectedPhoto"
        :show="showPhotoCard"
        @close="showPhotoCard = false"
      ></photo-card>
    </v-main>
  </v-app>
</template>

<script>
import login from "./components/login";
import fileUploader from "./components/file-uploader";
import photoCard from "./components/photo-card";

import ApiService from "./services/api.service";
import SiteUrl from "./settings/siteUrl.settings";

export default {
  name: "App",

  components: {
    login,
    fileUploader,
    photoCard,
  },
  created() {
    this.checkAuth();
  },
  data: () => ({
    selectedFiles: undefined,

    center: [50.0665774, 53.141024099999996],
    zoom: 11,
    rotation: 0,
    geolocPosition: undefined,
    selectedFeatures: [],

    points: [],
    selectedSrc: "",

    overlay: false,
    authed: false,
    user: {},
    photos: [],

    selectedPhoto: {},
    showPhotoCard: false,
  }),
  watch: {
    selectedFeatures(e) {
      this.getSourceByCoordinates(e[0]);
    },
  },
  methods: {
    getPhotoThumbUrl(photo) {
      return SiteUrl.photos() + "/" + photo.id + "/thumb";
    },
    async fetchPhotos() {
      this.points = [];

      let r = await ApiService.get(SiteUrl.photos());

      if (!r.error) {
        this.photos = r.data;
        this.photos.forEach((photo) => {
          this.points.push({
            id: photo.id,
            coordinates: [Number(photo.longitude), Number(photo.latitude)],
            date: new Date(photo.dateTime).toLocaleDateString(),
            data: photo,
          });
        });
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

<style scoped>
html,
body {
  height: 100%;
  overflow-y: hidden;
}

.h-100 {
  height: 100%;
}

.img-selected {
  max-width: 200px;
  max-height: 200px;
  margin: 0.5rem;
}

.profile-wrapper {
  position: absolute;
  right: 10px;
  top: 10px;
  z-index: 100;
  padding: 0.25rem;
}
</style>
