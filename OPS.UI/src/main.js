// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import jQuery from 'jquery';
import datatable from 'datatables.net';
import 'bootstrap';
import 'bootstrap/js/dist/util';
import 'bootstrap/js/dist/dropdown';
import Highcharts from 'highcharts';
import VueLazyload from 'vue-lazyload';
import Loading from 'vue-loading-overlay';
import 'vue-datatables-net';
import 'bootstrap-select';

import 'vue-loading-overlay/dist/vue-loading.css';

import Vue from 'vue';
import App from './App';
import router from './router';

window.$ = jQuery;
window.jQuery = jQuery;
window.$.fn.datatable = datatable;

window.$.extend(true, window.$.fn.dataTable.defaults, {
  processing: true,
  searching: false,
  destroy: true,
  ordering: false,
  serverSide: true,
  fixedHeader: true,
  saveState: true,
  lengthMenu: [[15, 100, 500, 1000], [15, 100, 500, 1000]],
  language: {
    url: 'https://cdn.datatables.net/plug-ins/1.10.21/i18n/Portuguese-Brasil.json',
  },
});

Highcharts.setOptions({
  global: {
    useUTC: false,
  },
  lang: {
    decimalPoint: ',',
    thousandsSep: '.',
  },
});

// https://github.com/hilongjw/vue-lazyload
Vue.use(VueLazyload, {
  preLoad: 1.3,
  error: '/static/images/sem_foto.jpg',
  loading: '/static/images/loading.svg',
  attempt: 1,
});

// https://jkchao.github.io/vue-loading/
Vue.use(Loading, {
  fullPage: true,
});

Vue.config.productionTip = false;

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  components: { App },
  template: '<App/>',
});
