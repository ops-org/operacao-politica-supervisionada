// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import jQuery from 'jquery';
import 'bootstrap';
import 'bootstrap/js/dist/util';
import 'bootstrap/js/dist/dropdown';
import Highcharts from 'highcharts';
import VueLazyload from 'vue-lazyload';
import Loading from 'vue-loading-overlay';
import VdtnetTable from 'vue-datatables-net';

import 'vue-loading-overlay/dist/vue-loading.css';

import Vue from 'vue';
import App from './App';
import router from './router';

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

Vue.use(VdtnetTable, {
  jquery: jQuery,
});

Vue.config.productionTip = false;

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  components: { App },
  template: '<App/>',
});
