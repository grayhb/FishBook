<template>
  <div>
    <div id="fishbook-map"></div>
    <div v-if="showCenterPoint" class="center-pointer"></div>
  </div>
</template>

<script>
import "ol/ol.css";

import Map from "ol/Map";
import View from "ol/View";
import TileLayer from "ol/layer/Tile";
import Feature from "ol/Feature";

import Select from "ol/interaction/Select";
import { click } from "ol/events/condition";

import { OSM, Vector as VectorSource } from "ol/source";

import VectorLayer from "ol/layer/Vector";
import { Circle as CircleStyle, Fill, Stroke, Style, Text } from "ol/style";
import { Point } from "ol/geom";

import { transform } from "ol/proj";

export default {
  props: ["items", "showCenterPoint"],
  mounted() {
    this.init();
  },
  watch: {
    items: {
      deep: true,
      handler: function() {
        this.refreshFeatures();
      },
    },
  },
  data: () => ({
    map: null,

    pointLayer: null,
    pointVectorSource: null,

    centerVectorSource: null,
    centerFeature: null,

    features: [],
  }),
  computed: {
    mapCenter() {
      if (!this.map) return [];

      let c = this.map.getView().getCenter();

      return transform(c, "EPSG:3857", "EPSG:4326");
    },
  },
  methods: {
    init() {
      var self = this;

      this.pointVectorSource = new VectorSource({
        features: this.features,
        projection: "EPSG:4326",
        wrapX: false,
      });

      this.pointLayer = new VectorLayer({
        source: this.pointVectorSource,
      });

      const raster = new TileLayer({
        source: new OSM(),
      });

      this.map = new Map({
        target: "fishbook-map",
        layers: [raster, this.pointLayer],

        view: new View({
          center: transform(
            [50.0665774, 53.141024099999996],
            "EPSG:4326",
            "EPSG:3857"
          ),
          zoom: 12,
        }),
      });

      // select interaction working on "click"
      const selectClick = new Select({
        condition: click,
        style: function(f) {
          return self.getPointStyle(f, true);
        },
      });

      selectClick.on("select", this.selectHandler);

      this.map.addInteraction(selectClick);

      this.refreshFeatures();
    },
    getPointStyle(feature, selected = false) {
      return new Style({
        image: new CircleStyle({
          radius: 8,
          fill: new Fill({ color: selected ? "#36a8ff" : "#d7e8f5" }),
          stroke: new Stroke({ color: "#1f608f" }),
        }),
        text: new Text({
          text: feature.get("text"),
          scale: 1,
          font: "12px monospace",
          offsetY: -14,
          fill: new Fill({
            color: "#000000",
          }),
        }),
      });
    },
    refreshFeatures() {
      this.features = [];

      this.items.forEach((item) => {
        let feature = new Feature({
          geometry: new Point(
            transform(
              [item.coordinates[0], item.coordinates[1]],
              "EPSG:4326",
              "EPSG:3857"
            )
          ),
          id: item.id,
          text: item.date,
        });

        feature.setStyle(this.getPointStyle(feature));

        this.features.push(feature);
      });

      this.pointVectorSource.clear();
      this.pointVectorSource.addFeatures(this.features);
    },
    selectHandler(e) {
      if (e.selected.length === 0) return;
      this.$emit("select", e.selected[0].get("id"));
    },
  },
};
</script>

<style scoped>
#fishbook-map {
  height: 99vh;
  overflow-y: hidden;
  position: absolute;
  width: 100%;
}

.center-pointer {
  z-index: 1;
  display: block;
  width: 10px;
  height: 10px;
  border: solid 1px #ff0000;
  border-radius: 5px;
  background-color: #ff0000;
  position: absolute;
  left: calc(50% - 4px);
  top: calc(50% - 9px);
}
</style>

<style>
.ol-zoom {
  display: none !important;
}

.ol-attribution {
  display: none !important;
}
</style>
