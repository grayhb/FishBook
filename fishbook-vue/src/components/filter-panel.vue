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
            outlined
            @change="commitChange"
          ></v-combobox>
          <v-combobox
            v-model="selectedMonth"
            clearable
            hide-selected
            multiple
            dense
            hide-details
            label="Месяц"
            :items="months"
            item-value="value"
            item-text="text"
            class="mt-4"
            outlined
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
            class="mt-4"
            outlined
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
          let y = new Date(photo.dateTime).getFullYear();
          let m = new Date(photo.dateTime).getMonth();
          if (
            (this.selectedYear.length === 0 ||
              this.selectedYear.indexOf(y) > -1) &&
            (this.selectedMonth.length === 0 ||
              this.selectedMonth.map((e) => e.value).indexOf(m) > -1)
          )
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

      items.sort();
      items.reverse();

      return items;
    },
    months() {
      let items = [];
      if (this.photos.length > 0) {
        this.photos.forEach((photo) => {
          let y = new Date(photo.dateTime).getFullYear();
          if (
            this.selectedYear === null ||
            this.selectedYear.length === 0 ||
            this.selectedYear.indexOf(y) > -1
          ) {
            let m = new Date(photo.dateTime).getMonth();
            if (items.indexOf(m) < 0) items.push(m);
          }
        });
      }

      items.sort();

      items = items.map((e) => {
        return { value: e, text: this.getMonthName(e) };
      });

      return items;
    },
  },
  data: () => ({
    selectedYear: [],
    selectedFish: [],
    selectedMonth: [],
  }),
  methods: {
    commitChange() {
      let filter = {
        years: this.selectedYear,
        fishs: this.selectedFish,
        months: this.selectedMonth.map((e) => e.value),
      };
      this.$emit("filtered", filter);
    },
    getMonthName(idx) {
      var objDate = new Date();
      objDate.setDate(1);
      objDate.setMonth(idx);

      var locale = "ru-RU",
        month = objDate.toLocaleString(locale, { month: "long" });

      return month;
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
