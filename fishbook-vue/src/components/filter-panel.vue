<template>
  <div class="filter-wrapper">
    <v-expansion-panels>
      <v-expansion-panel>
        <v-expansion-panel-header style="min-height: 30px">
          Фильтр
        </v-expansion-panel-header>
        <v-expansion-panel-content class="">
          <v-combobox
            v-model="selectedYear"
            clearable
            hide-selected
            multiple
            dense
            hide-details
            label="Год"
            :items="years"
            @change="commitChange"
          ></v-combobox>
          <v-combobox
            v-model="selectedFish"
            clearable
            hide-selected
            multiple
            dense
            hide-details
            label="Рыба"
            :items="fishs"
            class="mt-8"
            @change="commitChange"
          ></v-combobox>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
  </div>
</template>

<script>
export default {
  name: "filter-panel",
  props: ["photos"],
  computed: {
    fishs() {
      let items = [];
      if (this.photos.length > 0) {
        this.photos.forEach((photo) => {
          if (items.indexOf(photo.fishName) < 0) items.push(photo.fishName);
        });
      }
      items.sort();
      return items;
    },
    years() {
      let items = [];
      if (this.photos.length > 0) {
        this.photos.forEach((photo) => {
          let y = new Date(photo.dateTime).getFullYear();
          if (items.indexOf(y) < 0) items.push(y);
        });
      }
      return items;
    },
  },
  data: () => ({
    selectedYear: null,
    selectedFish: [],
  }),
  methods: {
    commitChange() {
      let filter = {
        years: this.selectedYear,
        fishs: this.selectedFish,
      };
      this.$emit("filtered", filter);
    },
  },
};
</script>

<style scoped>
.filter-wrapper {
  position: absolute;
  left: 190px;
  top: 0px;
  z-index: 7;
  padding: 0.25rem;
}
</style>
