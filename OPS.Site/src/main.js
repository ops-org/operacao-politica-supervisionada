// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import jQuery from 'jquery';
import datatable from 'datatables.net';
import VdtnetTable from 'vue-datatables-net';
import Highcharts from 'highcharts';
import VueLazyload from 'vue-lazyload';
import Loading from 'vue-loading-overlay';
import Multiselect from 'vue-multiselect';
import 'bootstrap-select';
import 'datatables.net-bs4';

import 'bootstrap/dist/css/bootstrap.min.css';
import 'font-awesome/css/font-awesome.min.css';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'vue-loading-overlay/dist/vue-loading.css';
import 'vue-multiselect/dist/vue-multiselect.min.css';

import Vue from 'vue';
import App from './App';
import router from './router';

require('dotenv').config();
require('bootstrap');

window.$ = jQuery;
window.jQuery = jQuery;
window.$.fn.datatable = datatable;

window.$.extend(true, window.$.fn.dataTable.defaults, {
  processing: true,
  searching: false,
  destroy: true,
  ordering: true,
  serverSide: true,
  fixedHeader: true,
  saveState: true,
  lengthMenu: [
    [15, 100, 500, 1000],
    [15, 100, 500, 1000]
  ],
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
  error: '//static.ops.org.br/sem_foto.jpg',
  loading: '//static.ops.org.br/loading.svg',
  attempt: 1,
});

// https://github.com/jkchao/vue-loading
Vue.use(Loading, {
  fullPage: true,
});


Vue.component('Multiselect', Multiselect);
Vue.component('VdtnetTable', VdtnetTable);

if(process.env.NODE_ENV === 'production'){
  Vue.config.productionTip = false;
  Vue.config.devtools = false;
}else{
  Vue.config.productionTip = true;
  Vue.config.devtools = true;
}

window.AddIfDontExists = function(arr, id, text) {
  if(arr.find(x => x.id == id) == null) {
    arr.push({id: id, text: text});
  }
}

window.GetIds = function(arr) {
  var ids = jQuery.map(arr, function (obj){
    return obj.id;
  });

  return ids;
}

new Vue({
  el: '#app',
  router,
  render: h => h(App)
})
