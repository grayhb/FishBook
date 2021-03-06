<template>
  <div>
    <v-dialog v-model="show" width="350" persistent @keydown.esc="closeDialog">
      <v-card>
        <v-card-title class="grey lighten-2 d-flex flex-nowrap px-3">
          <v-btn icon @click="onExport" class="mr-3">
            <v-icon>mdi-export-variant</v-icon>
          </v-btn>
          <v-spacer></v-spacer>

          <input
            v-if="!loading"
            type="datetime-local"
            v-model="item.dateTime"
          />

          <v-progress-circular
            v-else
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
      this.item = JSON.parse(JSON.stringify(this.data));
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
  }),
  methods: {
    customSave() {
      if (this.changed()) {
        this.$emit("created", this.item);
      }
    },
    selectedRawFile(e) {
      if (e && e.lastModified) {
        this.item.dateTime = new Date(e.lastModified)
          .toISOString()
          .substr(0, 16);
      }
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
        dateTime: this.item.dateTime,
      };
      this.$emit("updated", data);
    },
    changed() {
      let result = false;

      if (!this.isCustomCreate) {
        if (
          this.data.fishName !== this.item.fishName ||
          this.data.dateTime !== this.item.dateTime
        )
          result = true;
      } else {
        if (this.item.file) result = true;
      }
      return result;
    },
    onExport() {
      window.open(this.imageUrl, "_blank");
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
