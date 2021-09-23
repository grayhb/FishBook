<template>
  <div class="file-uploader-wrapper">
    <v-expansion-panels>
      <v-expansion-panel>
        <v-expansion-panel-header style="min-height: 30px">
          Загрузить фотографии
        </v-expansion-panel-header>
        <v-expansion-panel-content class="">
          <div class="d-flex">
            <v-btn icon v-if="files.length > 0" @click="files = []">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
            <v-file-input
              v-if="files.length < 1"
              v-model="rawFiles"
              label="Фотографии с gps"
              dense
              hide-details
              hide-input
              outlined
              multiple
              :disabled="loading"
              @change="filesChanged"
            ></v-file-input>
            <v-spacer></v-spacer>
            <v-btn
              depressed
              color="primary"
              class="mt-2"
              @click="uploadFiles"
              :loading="loading"
            >
              Загрузить
            </v-btn>
          </div>

          <div
            v-if="files.length > 0"
            class="mt-2 file-upload-details-container"
          >
            <div
              v-for="file in files"
              :key="file.id"
              class="file-upload-details"
            >
              <v-chip
                class="ml-2"
                :class="{
                  success: file.status === 'ok',
                  error: file.status === 'error',
                }"
                >{{ file.file.name }}</v-chip
              >
            </div>
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

      for (let file of this.files) {
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
  },
};
</script>
<style scoped>
.file-uploader-wrapper {
  position: absolute;
  z-index: 100;
  width: 300px;
  top: 100px;
  left: 10px;
}

.file-upload-details-container {
  max-height: 400px;
  overflow-y: auto;
}

.file-upload-details {
  font-size: 12px;
  padding: 5px 0;
}
</style>
