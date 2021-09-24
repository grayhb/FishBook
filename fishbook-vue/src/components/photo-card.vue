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
          <v-combobox
            v-model="fishName"
            :items="fishs"
            outlined
            dense
            hide-details
            class="mt-4"
            label="Наименование рыбы"
          ></v-combobox>
        </v-card-text>

        <v-divider></v-divider>

        <v-card-actions>
          <v-btn color="red--text" text @click="deleteItem">
            Удалить
          </v-btn>
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
  props: ["show", "data", "fishs"],
  watch: {
    show(e) {
      if (!e) return;
      this.fishName = this.data.fishName;
    },
  },
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
    fishName: "",
  }),
  methods: {
    closeDialog() {
      setTimeout(() => {
        if (!this.changed()) this.$emit("close");
        else this.updateItem();
      }, 100);
    },
    openFull() {
      this.showFullDialog = true;
    },
    deleteItem() {
      if (confirm("Удалить запись?")) {
        this.$emit("deleted");
      }
    },
    updateItem() {
      let data = {
        id: this.data.id,
        fishName: this.fishName,
      };
      this.$emit("updated", data);
    },
    changed() {
      let result = false;
      if (this.data.fishName !== this.fishName) result = true;
      return result;
    },
  },
};
</script>
