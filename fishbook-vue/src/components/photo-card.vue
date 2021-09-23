<template>
  <div>
    <v-dialog v-model="show" width="350" persistent @keydown.esc="closeDialog">
      <v-card>
        <v-card-title class="text-h5 grey lighten-2">
          {{ date }}
        </v-card-title>

        <v-card-text
          class="pt-5 d-flex align-center justify-center flex-column"
        >
          <img :src="thumbUrl" @click="openFull" />
        </v-card-text>

        <v-divider></v-divider>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" text @click="closeDialog">
            Закрыть
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-dialog
      v-model="showFullDialog"
      fullscreen
      hide-overlay
      @keydown.esc="showFullDialog = false"
    >
      <v-img
        :src="imageUrl"
        @click="showFullDialog = false"
        height="100vh"
        width="100vw"
        contain
      ></v-img>
    </v-dialog>
  </div>
</template>

<script>
import SiteUrl from "../settings/siteUrl.settings";

export default {
  name: "photo-card",
  props: ["show", "data"],
  computed: {
    thumbUrl() {
      return `${this.imageUrl}/thumb`;
    },
    imageUrl() {
      return `${SiteUrl.photos()}/${this.data.id}`;
    },
    date() {
      return new Date(this.data.dateTime).toLocaleDateString();
    },
  },
  data: () => ({
    showFullDialog: false,
  }),
  methods: {
    closeDialog() {
      this.$emit("close");
    },
    openFull() {
      this.showFullDialog = true;
      //let url = `${SiteUrl.photos()}/${this.data.id}`;
      //window.open(url, "_blank").focus();
    },
  },
};
</script>
