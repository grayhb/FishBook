<template>
  <v-app>
    <v-main>
      <login v-if="!authed"></login>

      <div v-else class="profile-wrapper">
        <v-chip>{{ user.displayName }}</v-chip>
      </div>

      <file-uploader v-if="authed"></file-uploader>

      <!-- <v-file-input 
        v-model="selectedFiles"
        @change="fileSelected"
        multiple
      ></v-file-input> -->

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

        <vl-feature v-for="point in points" :key="point.name">
          <vl-geom-point :coordinates="point.coordinates"></vl-geom-point>

          <vl-style-box>
            <vl-style-circle :radius="7">
              <vl-style-fill color="blue"></vl-style-fill>
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

        <!-- 
      <vl-overlay v-for="feature in selectedFeatures" :key="feature.id" :id="feature.id + '-popup'" :position="feature.geometry.coordinates">
        <div style="background: #fff">
          <img :src="selectedSrc" class="img-selected" @click="overlay = true"/>
        </div>
      </vl-overlay> -->
      </vl-map>
      <!-- 
    <div>
      {{geolocPosition}}
      count - {{points.length}}
      selected - {{selectedFeatures.length}}
    </div> -->
      <!-- 
    <v-overlay
        :value="overlay"
      >
      <div class="d-flex flex-column">
      <v-img
        contain
        height="90vh" width="90vw"
        :src="selectedSrc"
      ></v-img>

        <v-btn
          color="success"
          class="mt-2"
          @click="overlay = false"
        >
          Закрыть
        </v-btn>
        </div>
      </v-overlay> -->
    </v-main>
  </v-app>
</template>

<script>
var EXIF = require("exif-js/exif");

import login from "./components/login";
import fileUploader from "./components/file-uploader";

import ApiService from "./services/api.service";
import SiteUrl from "./settings/siteUrl.settings";

export default {
  name: "App",

  components: {
    login,
    fileUploader,
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
  }),
  watch: {
    selectedFeatures(e) {
      this.getSourceByCoordinates(e[0]);
    },
  },
  methods: {
    async checkAuth() {
      let r = await ApiService.get(SiteUrl.profile());

      if (!r.error) {
        this.authed = true;
        this.user = r.data;
      }
    },

    fileSelected(e) {
      this.points = [];

      var self = this;

      e.forEach((f) => {
        EXIF.getData(f, function() {
          let allMetaData = EXIF.getAllTags(this);
          let c = self.coords(allMetaData);

          self.points.push({
            file: f,
            metadata: allMetaData,
            coordinates: [c.lon, c.lat],
            dateTime: self.dateTimeNormilize(allMetaData.DateTime),
            date: self.dateTimeNormilize(allMetaData.DateTime).split(" ")[0],
          });
        });
      });
    },
    dateTimeNormilize(e) {
      let tmp = e.split(" ");

      let tmpDate = tmp[0].split(":");

      let y = tmpDate[0];
      let m = tmpDate[1];
      let d = tmpDate[2];

      return d + "." + m + "." + y + " " + tmp[1];
    },
    coords(e) {
      if (!e.GPSLongitude || !e.GPSLatitude) return null;

      return {
        lon: this.DD(e.GPSLongitude[0], e.GPSLongitude[1], e.GPSLongitude[2]),
        lat: this.DD(e.GPSLatitude[0], e.GPSLatitude[1], e.GPSLatitude[2]),
      };
    },
    DD(d, m, s) {
      return d + m / 60 + s / 3600;
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
      var self = this;

      let reader = new FileReader();
      reader.onloadend = function() {
        self.selectedSrc = reader.result;
        self.overlay = true;
      };
      reader.readAsDataURL(point.file);
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
