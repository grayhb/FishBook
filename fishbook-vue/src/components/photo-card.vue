<template>
  <div>
    <v-dialog v-model="show" width="350" persistent @keydown.esc="closeDialog">
      <v-card>
        <v-card-title class="text-h5 grey lighten-2">
          <span>{{ date }}</span>
          <v-spacer></v-spacer>
          <v-progress-circular
            v-if="loading"
            indeterminate
            color="primary"
          ></v-progress-circular>
        </v-card-title>

        <v-card-text
          class="pt-5 d-flex align-center justify-center flex-column"
        >
          <img
            v-if="!isCustomCreate"
            :src="thumbUrl"
            @click="openFull"
            style="cursor: pointer"
            class="photo-thumb"
          />

          <v-file-input
            v-else
            v-model="item.file"
            label="Фото без gps"
            dense
            hide-details
            outlined
            :disabled="loading"
            class="w-100"
            @change="selectedRawFile"
            accept="image/*"
          ></v-file-input>

          <v-combobox
            v-model="item.fishName"
            :items="fishs"
            outlined
            dense
            hide-details
            class="mt-4 w-100"
            label="Наименование рыбы"
          ></v-combobox>
        </v-card-text>

        <v-divider></v-divider>

        <v-card-actions>
          <v-btn
            v-if="!isCustomCreate"
            color="red--text"
            text
            @click="deleteItem"
            :disabled="loading"
          >
            Удалить
          </v-btn>
          <v-spacer></v-spacer>

          <v-btn color="primary" text @click="closeDialog" :disabled="loading">
            Закрыть
          </v-btn>
          <v-btn
            v-if="isCustomCreate"
            color="teal"
            text
            @click="customSave"
            :disabled="loading"
          >
            Сохранить
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
  props: ["show", "data", "fishs", "loading"],
  watch: {
    show(e) {
      if (!e) return;
      this.item = Object.assign({}, this.data);
      if (this.isCustomCreate) this.rawFile = null;
    },
  },
  computed: {
    thumbUrl() {
      return `${this.imageUrl}/thumb`;
    },
    imageUrl() {
      return `${SiteUrl.photos()}/${this.item.id}`;
    },
    date() {
      let d = new Date(this.item.dateTime);
      return `${d.toLocaleDateString()} ${d.toLocaleTimeString()}`;
    },
    isCustomCreate() {
      return this.item.customCreate;
    },
  },
  data: () => ({
    showFullDialog: false,
    item: {},
    //rawFile: null,
  }),
  methods: {
    customSave() {
      if (this.changed()) {
        this.$emit("created", this.item);
      }
    },
    selectedRawFile(e) {
      if (e) {
        this.item.dateTime = new Date(e.lastModified).toISOString();
        console.log(e);
      } else this.item.dateTime = new Date().toISOString();
    },
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
        id: this.item.id,
        fishName: this.item.fishName,
      };
      this.$emit("updated", data);
    },
    changed() {
      let result = false;

      if (!this.isCustomCreate) {
        if (this.data.fishName !== this.item.fishName) result = true;
      } else {
        if (this.item.file) result = true;
      }
      return result;
    },
  },
};
</script>
<style scoped>
.photo-thumb {
  max-width: 300px;
  max-height: 300px;
}

.w-100 {
  width: 100%;
}
</style>
