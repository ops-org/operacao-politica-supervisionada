<template>
  <select class="form-control">
    <option v-for="option in options" v-bind:value="option.id" v-bind:data-tokens="option.tokens" :key="option.id" >{{ option.text }}</option>
  </select>
</template>

<script>
import jQuery from 'jquery';

// const axios = require('axios');

export default {
  // twoWay: true,
  props: {
    options: {
      type: Array,
      default: () => [],
    },
    value: {
      type: [Number, String, Array],
      default: null,
    },
  },
  mounted() {
    const vm = this;

    const $select = jQuery(this.$el).selectpicker({
      width: '100%',
      liveSearch: true,
      liveSearchNormalize: true,
      selectOnTab: true,
      selectedTextFormat: 'count > 3',
      maxOptions: 50,
      style: 'btn-light',
      noneSelectedText: 'Nada selecionado',
      noneResultsText: 'Nenhum resultado encontrado {0}',
      selectAllText: 'Selecionar tudo',
      deselectAllText: 'Deselecionar tudo',
    });

    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry/i.test(window.navigator.userAgent)) {
      $select.selectpicker('mobile');
    }

    $select.on('change', () => {
      vm.$emit('input', $select.val());
    });
  },
  watch: {
    value(v) {
      const vm = this;

      this.$nextTick(() => {
        jQuery(vm.$el).val(v).selectpicker('refresh');
      });
    },
    options() {
      const vm = this;

      this.$nextTick(() => {
        jQuery(vm.$el).val(vm.value).selectpicker('refresh');
      });
    },
  },
  destroyed() {
    jQuery(this.$el).off().selectpicker('destroy');
  },
};
</script>

<style>
  @import url('https://cdn.jsdelivr.net/npm/bootstrap-select@1.13.17/dist/css/bootstrap-select.min.css');
</style>
