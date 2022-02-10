<template>
  <div class="file-uploader-wrapper">
    <v-expansion-panels>
      <v-expansion-panel>
        <v-expansion-panel-header style="min-height: 30px">
          Загрузить
        </v-expansion-panel-header>
        <v-expansion-panel-content class="file-uploader-container">
          <div class="d-flex">
            <v-btn @click="mapPointHandler" icon color="primary" class="mr-2">
              <v-icon v-if="!mapPoint">mdi-map-marker</v-icon>
              <v-icon v-else>mdi-checkbox-marked-circle</v-icon>
            </v-btn>

            <v-btn icon v-if="files.length > 0" @click="files = []">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
            <v-file-input
              v-if="files.length < 1"
              v-model="rawFiles"
              label="Фото с gps"
              dense
              hide-details
              hide-input
              outlined
              multiple
              :disabled="loading"
              @change="filesChanged"
              accept="image/*"
            ></v-file-input>
            <v-spacer></v-spacer>
            <v-btn
              depressed
              color="primary"
              @click="uploadFiles"
              :loading="loading"
              :disabled="files.length < 1"
              icon
            >
              <v-icon>
                mdi-upload
              </v-icon>
            </v-btn>
          </div>

          <div
            v-if="files.length > 0"
            class="mt-2 file-upload-details-container"
          >
            <v-chip
              v-for="file in files"
              :key="file.id"
              x-small
              label
              class="w-100"
              :class="{
                success: file.status === 'ok',
                error: file.status === 'error',
              }"
              >{{ file.file.name }}</v-chip
            >
          </div>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
  </div>
</template>

<script>
import ApiService from "../services/api.service";
import SiteUrl from "../settings/siteUrl.settings";

var EXIF = require("exif-js/exif");

export default {
  name: "file-uploader",
  data: () => ({
    rawFiles: [],
    files: [],
    loading: false,
    mapPoint: false,
  }),
  methods: {
    filesChanged() {
      this.files = [];

      var self = this;
      this.rawFiles.forEach((f, i) => {
        EXIF.getData(f, function() {
          let allMetaData = EXIF.getAllTags(this);
          let c = self.coords(allMetaData);
          if (c) {
            self.files.push({
              id: i,
              file: f,
              longitude: c.lon,
              latitude: c.lat,
              dateTime: self.dateTimeNormilize(allMetaData.DateTime),
              status: "",
            });
          }
        });
      });
    },
    async uploadFiles() {
      this.loading = true;

      for (let file of this.files.filter((e) => !e.status)) {
        var body = new FormData();
        body.append("file", file.file);
        body.append("longitude", file.longitude);
        body.append("latitude", file.latitude);
        body.append("dateTime", file.dateTime);

        let r = await ApiService.post(SiteUrl.upload(), body);

        if (r.error) {
          alert(r.error);
          file.status = "error";
          continue;
        }
        file.status = "ok";
      }

      this.$emit("uploaded");
      this.loading = false;
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
    mapPointHandler() {
      this.mapPoint = !this.mapPoint;
      this.$emit("change-map-point", this.mapPoint);
    },
  },
};
</script>
<style scoped>
.w-100 {
  width: 100%;
}
.file-uploader-wrapper {
  position: absolute;
  z-index: 100;
  width: 180px;
  top: 5px;
  left: 5px;
}

.file-upload-details-container {
  max-height: 400px;
  overflow-y: auto;
}

.file-upload-details {
}
</style>

<style>
.file-uploader-wrapper .v-expansion-panel-content__wrap {
  padding: 0 10px 10px 10px;
}
</style>
